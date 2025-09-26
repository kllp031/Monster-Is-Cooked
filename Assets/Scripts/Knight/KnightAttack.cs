using UnityEngine;
using UnityEngine.InputSystem;

public class KnightAttack : MonoBehaviour
{
    public InputActionAsset actions;
    private InputAction attack;

    [SerializeField] private GameObject attackEffect;

    private void OnEnable()
    {
        var map = actions?.FindActionMap("Player", throwIfNotFound: false);
        attack = map?.FindAction("Attack", throwIfNotFound: false);
        map?.Enable();
        attack?.Enable();
    }

    private void OnDisable()
    {
        attack?.Disable();
        actions?.FindActionMap("Player", throwIfNotFound: false)?.Disable();
    }

    private void Update()
    {
        if (attack != null && attack.WasPressedThisFrame())
        {
            // Enable an existing in-scene effect object
            if (attackEffect != null)
            {
                attackEffect.SetActive(true);
                attackEffect.GetComponent<Animator>().SetTrigger("Attack");
            }
        }
    }
}
