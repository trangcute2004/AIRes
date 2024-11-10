using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    public int TableNumber { get; private set; }
    public int Capacity { get; private set; }
    public bool IsOccupied { get; private set; }
    public bool IsDirty { get; private set; }

    // Set table number and capacity
    public void InitializeTable(int tableNumber, int capacity)
    {
        TableNumber = tableNumber;
        Capacity = capacity;
        IsOccupied = false; // Set table as unoccupied initially
        IsDirty = false;    // Set table as clean initially
    }

    // Example of methods to occupy and vacate the table
    public void Occupy() => IsOccupied = true;
    public void Vacate() => IsOccupied = false;

    // Example of cleaning the table
    public void SetDirty() => IsDirty = true;
    public void Clean() => IsDirty = false;
}
