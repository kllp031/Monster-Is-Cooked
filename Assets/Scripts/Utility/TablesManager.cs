using System;
using System.Collections.Generic;
using UnityEngine;

public class TablesManager : MonoBehaviour
{
    [SerializeField] List<TableDetail> tablesList = new(); // Assign differnt list for each level

    private static TablesManager instance = null;

    public static TablesManager Instance { get => instance; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    private void OnEnable()
    {
        LoadTablesPath();
    }

    private void LoadTablesPath()
    {
        tablesList.Clear();
        for(int i = 0; i < transform.childCount; i++)
        {
            Path path = transform.GetChild(i).GetComponent<Path>();
            if (path == null) continue;
            tablesList.Add(new TableDetail(path));
        }
    }
    // Customers will return their assigned table when moving out
    public void ReturnTable(Customer customer)
    {
        foreach (var table in tablesList)
        {
            if (table != null && table.AssignedCustomer != null)
            {
                if (table.AssignedCustomer == customer) table.AssignedCustomer = null;
            }
        }
    }

    // Customers Spawner will consistently ask TablesManager for unoccupied table
    public TableDetail GetAvailableTable()
    {
        foreach (var table in tablesList)
        {
            if (table == null) continue;
            if (table.AssignedCustomer == null || table.AssignedCustomer.gameObject.activeInHierarchy == false) return table;
        }
        return null;
    }
}

[Serializable]
public class TableDetail
{
    [SerializeField] Path pathToTable;
    [SerializeField] private Customer assignedCustomer;

    public TableDetail(Path path)
    {
        pathToTable = path;
    }

    public Path PathToTable { get => pathToTable; }
    public Customer AssignedCustomer { get => assignedCustomer; set => assignedCustomer = value; }
}
