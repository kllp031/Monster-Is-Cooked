using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Online version of <see cref="HotbarManager"/>. Lưu trữ <see cref="Recipe"/>
/// trực tiếp thay vì <see cref="Food"/> gameobject, vì online flow:
/// - Cooked food: <see cref="CookingManager"/> spawn local Food → manager
///   trích recipe + destroy gameobject (không cần giữ).
/// - Pickup networked: <see cref="FoodHolderOnline.PickUpFromWorld"/> add
///   recipe trực tiếp, không cần local Food.
/// - Drop / throw: <see cref="FoodHolderOnline"/> đọc recipe từ slot đang
///   chọn → spawn <see cref="FoodOnline"/> networked.
///
/// Singleton scene-resident. KHÔNG đặt cùng scene với offline
/// <see cref="HotbarManager"/> — cả hai sẽ subscribe vào
/// <see cref="CookingManager.OnFoodSpawned"/> và xử lý trùng nhau.
/// </summary>
public class HotbarManagerOnline : MonoBehaviour
{
    public static HotbarManagerOnline Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxSlots = 5;

    private List<Recipe> slots;
    private int selectedSlotIndex = 0;

    public event Action OnHotbarUpdated;
    public event Action<int> OnSelectionChanged;

    public Recipe CurrentRecipe =>
        IsValidIndex(selectedSlotIndex) ? slots[selectedSlotIndex] : null;

    public int SelectedSlotIndex => selectedSlotIndex;
    public int MaxSlots => maxSlots;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        slots = new List<Recipe>();
        for (int i = 0; i < maxSlots; i++) slots.Add(null);
    }

    private void Start()
    {
        SelectSlot(selectedSlotIndex);

        if (CookingManager.Instance != null)
            CookingManager.Instance.OnFoodSpawned.AddListener(HandleCookedFood);
        else
            //Debug.LogWarning($"{nameof(HotbarManagerOnline)}: CookingManager.Instance null tại Start.");

        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelEnd.AddListener(HandleLevelEnd);
    }

    private void OnDisable()
    {
        if (CookingManager.Instance != null)
            CookingManager.Instance.OnFoodSpawned.RemoveListener(HandleCookedFood);
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelEnd.RemoveListener(HandleLevelEnd);
    }

    private void HandleCookedFood(Food food)
    {
        if (food == null) return;
        bool added = AddRecipe(food.Recipe);
        // Local Food gameobject xong việc — trích recipe rồi destroy luôn.
        // Online không dùng Food gameobject để giữ inventory state.
        Destroy(food.gameObject);
        //if (!added) Debug.Log($"{nameof(HotbarManagerOnline)}: hotbar đầy, recipe bị bỏ.");
    }

    private void HandleLevelEnd(bool _)
    {
        for (int i = 0; i < slots.Count; i++) slots[i] = null;
        OnHotbarUpdated?.Invoke();
    }

    public bool AddRecipe(Recipe recipe)
    {
        if (recipe == null) return false;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = recipe;
                OnHotbarUpdated?.Invoke();
                return true;
            }
        }
        return false;
    }

    public void RemoveRecipeAt(int index)
    {
        if (!IsValidIndex(index)) return;
        if (slots[index] == null) return;
        slots[index] = null;
        OnHotbarUpdated?.Invoke();
    }

    public void RemoveSelectedRecipe() => RemoveRecipeAt(selectedSlotIndex);

    public void SelectSlot(int index)
    {
        if (!IsValidIndex(index)) return;
        selectedSlotIndex = index;
        OnSelectionChanged?.Invoke(selectedSlotIndex);
    }

    public Recipe GetRecipeAt(int index) =>
        IsValidIndex(index) ? slots[index] : null;

    public bool IsHotbarFull()
    {
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] == null) return false;
        return true;
    }

    private bool IsValidIndex(int index) => index >= 0 && index < slots.Count;
}
