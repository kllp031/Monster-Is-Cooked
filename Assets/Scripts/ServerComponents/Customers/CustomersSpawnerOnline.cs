using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomersSpawnerOnline : NetworkBehaviour
{
    #region Singleton
    private static CustomersSpawnerOnline instance = null; // Each device has its own instance
    public static CustomersSpawnerOnline Instance { get => instance; }
    #endregion

    [SerializeField] CustomerOnline customerPrefab;
    [SerializeField] RecipeGallery recipeGallery;
    [SerializeField] UnityEvent onCustomerStarted = new();

    [Networked] bool CustomerStarted { get; set; }
    [Networked, Capacity(20)] 
    NetworkLinkedList<NetworkString<_32>> CustomersSkinIds { get; } // Skins to appear in this game
    [Networked, Capacity(20)] 
    NetworkLinkedList<CustomerDetailOnline> TempCustomerDetails { get; }  // A copy of customer details to spawn
    [Networked, Capacity(20)] 
    NetworkLinkedList<float> TempAppearTime { get; } // A copy of appear time
    [Networked, Capacity(20)]
    NetworkLinkedList<NetworkId> ActiveCustomers { get; } // Customers who haven't left yet

    private List<CustomerOnline> spawnedCustomer = new(); // All spawned customers, used for pooling

    public RecipeGallery RecipeGallery { get => recipeGallery; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        if (GameManagerOnline.Instance != null)
        {
            GameManagerOnline.OnLevelStarted += OnLevelStart;
            GameManagerOnline.OnLevelEnd += OnLevelEnd;
        }
    }

    public override void Spawned()
    {
        base.Spawned();
    }

    public void OnLevelStart()
    {
        Debug.Log("Customer spawner on level start!");
        spawnedCustomer.Clear();

        if (Runner != null && Runner.IsSharedModeMasterClient)
        {
            CustomerStarted = false;

            CustomersSkinIds.Clear();
            var rawCustomerSkinIds = GameManagerOnline.Instance.GetCurrentLevelDetail().CustomersSkinIds;
            foreach(var skinId in rawCustomerSkinIds) CustomersSkinIds.Add(skinId);

            TempCustomerDetails.Clear();
            var rawCustomerDetails = GameManagerOnline.Instance.GetCurrentLevelDetail().CustomerDetails;
            foreach(var customerDetails in rawCustomerDetails)
            {
                TempCustomerDetails.Add(customerDetails);
            }

            TempAppearTime.Clear();
            var rawAppearTime = GameManagerOnline.Instance.GetCurrentLevelDetail().AppearTime;
            foreach(var appearTime in rawAppearTime) TempAppearTime.Add(appearTime);

            ActiveCustomers.Clear();
        }
    }

    public void OnLevelEnd(bool bruh)
    {
        // Force to clear all the customers in case the level ends because the player died
        if (Runner != null && Runner.IsSharedModeMasterClient)
        {
            for(int i = spawnedCustomer.Count - 1; i >=0 ; i--)
            {
                var customer = spawnedCustomer[i];
                if (customer == null) continue;
                Runner.Despawn(customer.Object);
            }
            ActiveCustomers.Clear();
        }

        spawnedCustomer.Clear();
    }

    public override void FixedUpdateNetwork()
    {
        //Debug.Log("Update!");
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        //Debug.Log("Runner is master client!");

        // Use GameManagerOnline instead!!!!!!!!!
        if (GameManagerOnline.Instance == null) { Debug.LogWarning("GameManager is not found in this scene!"); return; }
        //else if (!GameManagerOnline.Instance.LevelStarted || !GameManagerOnline.Instance.GameStarted) return;

        if (TablesManagerOnline.Instance == null) { Debug.LogWarning("Tables Manager Online is not found in this scene!"); return; }

        if (CustomersSkinIds.Count == 0) { Debug.LogWarning("Please assign some customerSkins to spawn Customer!"); }

        float elapsedTime = Runner.SimulationTime - GameManagerOnline.Instance.LevelStartTime; // Use GameManagerOnline to store LEVEL START TIME!!!

        // All the customers have been spawned
        if (TempAppearTime.Count == 0 || TempCustomerDetails.Count == 0)
        {
            // If there are no active customers left -> End Game
            if (ActiveCustomers.Count == 0) GameManagerOnline.Instance.EndLevel();
            return;
        }

        // If the customer at the top of the appear time list is ready to be spawned
        if (elapsedTime >= TempAppearTime[0])
        {
            TableDetailOnline availableTable;
            if (!TablesManagerOnline.Instance.GetAvailableTable(out availableTable)) return;

            if (!CustomerStarted)
            {
                onCustomerStarted.Invoke();
                CustomerStarted = true;
            }

            // Get customer detail
            int randomIndex = Random.Range(0, TempCustomerDetails.Count);
            CustomerDetailOnline newDetail = TempCustomerDetails[randomIndex];
            TempCustomerDetails.Remove(newDetail);
            string newSkinId = null;

            // Get customer skin
            if (CustomersSkinIds.Count > 0) newSkinId = CustomersSkinIds[Random.Range(0, CustomersSkinIds.Count)].ToString();

            // Spawn Customer
            CustomerOnline newCustomer = null;
            foreach (var customer in spawnedCustomer)
            {
                if (!customer.IsActivated)
                {
                    newCustomer = customer;
                    break;
                }
            }
            if (newCustomer == null)
            {
                newCustomer = Runner.Spawn(customerPrefab, onBeforeSpawned: (runner, networkObject) => {
                    InitCustomer(networkObject, newDetail, newSkinId, availableTable);
                });
            }
            else
            {
                InitCustomer(newCustomer.Object, newDetail, newSkinId, availableTable);
            }
            TempAppearTime.Remove(TempAppearTime[0]);
        }
    }

    private void InitCustomer(NetworkObject customerNetworkObj, CustomerDetailOnline newDetail, string newSkinId, TableDetailOnline availableTable)
    {
        if (customerNetworkObj == null) return;
        Debug.Log("Init customer");
        TablesManagerOnline.Instance.AssignCustomer(availableTable.ID, customerNetworkObj.Id);
        if (customerNetworkObj.TryGetComponent<CustomerOnline>(out CustomerOnline newCustomer))
        {
            newCustomer.SetDetail(newDetail); // Assign customer details to customer
            newCustomer.SetSkin(newSkinId); // Assign skin to customer
            newCustomer.SetTablePath(availableTable.PathToTable); // Assign path to customer Path should be a struct for synchronization
            newCustomer.SetActivated(true);
        }

        ActiveCustomers.Add(customerNetworkObj.Id);
    }

    private void OnDisable()
    {
        if (GameManagerOnline.Instance != null) GameManagerOnline.OnLevelStarted -= OnLevelStart;
        if (GameManagerOnline.Instance != null) GameManagerOnline.OnLevelEnd -= OnLevelEnd;
    }

    public void OnCustomerSpawned(CustomerOnline customer)
    {
        if (customer == null) return;
        if (!spawnedCustomer.Contains(customer)) { spawnedCustomer.Add(customer); }
    }

    // This function is called by the customer to remove itself from the active customers list
    public void OnCustomerLeft(CustomerOnline customer)
    {
        Debug.Log("Customer left: " + customer);
        if (Runner == null || !Runner.IsSharedModeMasterClient || customer == null) return;
        foreach (var activeCustomer in ActiveCustomers)
        {
            if (activeCustomer == customer.Object.Id)
            {
                ActiveCustomers.Remove(customer.Object.Id);
                break;
            }
        }
        Runner.Despawn(customer.Object);
    }
}
