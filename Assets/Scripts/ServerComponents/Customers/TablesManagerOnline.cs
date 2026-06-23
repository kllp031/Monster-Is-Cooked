using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TablesManagerOnline : NetworkBehaviour
{
    #region Singleton
    private static TablesManagerOnline instance = null;
    public static TablesManagerOnline Instance { get => instance; }
    #endregion

    [Networked, Capacity(50)]
    NetworkLinkedList<TableDetailOnline> tablesList => default; //Automatically assign table paths

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;
    }

    public override void Spawned()
    {
        if (Runner != null && Runner.IsSharedModeMasterClient)
        {
            LoadTablesPath();
        }
    }

    private void LoadTablesPath()
    {
        tablesList.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Path path = transform.GetChild(i).GetComponent<Path>();
            if (path == null) continue;
            tablesList.Add(new TableDetailOnline(i, path));
        }
    }
    public bool AssignCustomer(int tableId, NetworkId customer)
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return false;
        for (int i = 0; i < tablesList.Count; i++)
        {
            if (tablesList[i].ID == tableId)
            {
                var tables = tablesList; 
                var table = tables[i];
                table.AssignedCustomer = customer;
                tables[i] = table;
                return true;
            }
        }
        return false;
    }

    // Customers will return their assigned table when moving out
    public void ReturnTable(NetworkId customer)
    {
        if (Runner == null || !Runner.IsSharedModeMasterClient) return;
        for (int i = 0; i < tablesList.Count; i++)
        {
            if (tablesList[i].AssignedCustomer == customer)
            {
                var tables = tablesList;
                var table = tables[i];
                table.IsAssigned = false;
                tables[i] = table;
                break;
            }
        }
    }
    
    // Customers Spawner will consistently ask TablesManager for unoccupied table
    public bool GetAvailableTable(out TableDetailOnline avaliableTable)
    {
        foreach (var table in tablesList)
        {
            if (!table.IsAssigned)
            {
                avaliableTable = table;
                return true;
            }
        }
        avaliableTable = new();
        return false;
    }
}
[Serializable]
public struct TableDetailOnline : INetworkStruct
{
    [SerializeField] int id;
    [SerializeField] PathDetails pathToTable;
    [SerializeField] bool isAssigned;
    [SerializeField] private NetworkId assignedCustomer;

    public TableDetailOnline(int id, Path path)
    {
        this.id = id;
        isAssigned = false;
        pathToTable = new(path);
        assignedCustomer = default;
    }

    public int ID { get => id; }
    public PathDetails PathToTable { get => pathToTable; }
    public bool IsAssigned { get => isAssigned; set => isAssigned = value; }
    public NetworkId AssignedCustomer 
    { 
        get => assignedCustomer; 
        set
        {
            if (assignedCustomer != value) isAssigned = true;
            assignedCustomer = value;
        }
    }
}

[Serializable]
public struct PathDetails : INetworkStruct
{
    [Networked] public int PointCount { get; set; }
    [Networked, Capacity(20)]
    public NetworkArray<Vector2> Points => default;
    public PathDetails(Path path)
    {
        PointCount = 0;
        if (path == null) return;
        for(int i = 0; i < ((path.Points.Count < 20) ? path.Points.Count : 20); i++)
        {
            Points.Set(i, path.Points[i]);
            // //Debug.Log("Set point " + path.Points[i] + " to index " + i);
            PointCount++;
        }
    }
}
