using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D.Animation;

public class CustomersSpawner : MonoBehaviour
{
    private static CustomersSpawner instance = null;
    [SerializeField] Customer customerPrefab;
    [SerializeField] List<SpriteLibraryAsset> customerSkins = new();
    [SerializeField] List<CustomerDetail> customerDetails = new();
    [Tooltip("The customer will appear 'appear time' seconds after the start of the level")]
    [SerializeField] List<float> appearTime = new();
    [SerializeField] UnityEvent updateTimeBeforeGameStart = new();
    [SerializeField] UnityEvent onGameStarted = new();

    private bool gameStarted = false;
    private List<CustomerDetail> tempCustomerDetails = new();  // A copy of customer details
    private List<float> tempAppearTime = new(); // A copy of appear time
    [SerializeField] private List<Customer> spawnedCustomer = new(); // All spawned customers
    private List<Customer> activeCustomers = new(); // Customers who haven't left yet

    public static CustomersSpawner Instance { get => instance; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void OnEnable()
    {
        gameStarted = false;
        tempCustomerDetails = new List<CustomerDetail>(customerDetails);
        tempAppearTime = new List<float>(appearTime);
        spawnedCustomer.Clear();
        activeCustomers.Clear();
    }

    private void OnValidate()
    {
        if (customerDetails.Count != appearTime.Count) Debug.LogWarning("List of Customer Details and List of Appear Time must have the same count!");
    }

    private void Update()
    {
        if (customerSkins.Count == 0) { Debug.LogWarning("Please assign some customerSkins to spawn Customer!"); }

        if (GameManager.Instance == null) { Debug.LogWarning("GameManager is not found in this scene!"); return; }
        float elapsedTime = Time.time - GameManager.Instance.LevelStartTime;

        // All the customers have been spawned
        if (tempAppearTime.Count == 0 || tempCustomerDetails.Count == 0)
        {
            // If there are no active customers left -> End Game
            if (activeCustomers.Count == 0) GameManager.Instance.EndLevel();
            return;
        }

        // If the customer at the top of the appear time list is ready to be spawned
        if (elapsedTime >= tempAppearTime[0])
        {
            TableDetail table = TablesManager.Instance.GetAvailableTable();
            if (table == null) return; // There are no unoccupied tables

            if (!gameStarted)
            {
                onGameStarted.Invoke();
                gameStarted = true;
            }

            // Spawn Customer
            Customer newCustomer = null;
            foreach(var customer in spawnedCustomer)
            {
                if (!customer.gameObject.activeInHierarchy)
                {
                    newCustomer = customer;
                    break;
                }
            }
            if (newCustomer == null)
            {
                newCustomer = Instantiate(customerPrefab);
                spawnedCustomer.Add(newCustomer);
            }

            // Get customer detail
            int randomIndex = Random.Range(0, tempCustomerDetails.Count);
            CustomerDetail newDetail = tempCustomerDetails[randomIndex];
            tempCustomerDetails.RemoveAt(randomIndex);
            SpriteLibraryAsset newSkin = null;

            // Get customer skin
            if (customerSkins.Count > 0) newSkin = customerSkins[Random.Range(0, customerSkins.Count)];

            // Assign all necessary information
            table.AssignedCustomer = newCustomer; // Assign that table to the customer
            newCustomer.SetDetail(newDetail); // Assign customer details to customer
            newCustomer.SetSkin(newSkin); // Assign skin to customer
            newCustomer.SetTablePath(table.PathToTable); // Assign path to customer

            newCustomer.gameObject.SetActive(true);
            newCustomer.Activate();
            activeCustomers.Add(newCustomer);
            tempAppearTime.RemoveAt(0);
        }
    }

    // This function is called by the customer to remove itself from the active customers list
    public void OnCustomerLeft(Customer customer)
    {
        foreach(var activeCustomer in activeCustomers)
        {
            if (activeCustomer == customer)
            {
                activeCustomers.Remove(customer);
                return;
            }
        }
    }
}
