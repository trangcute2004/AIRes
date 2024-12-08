using UnityEngine;

public class Table : MonoBehaviour
{
    public int TableNumber { get; private set; }
    public int Capacity { get; private set; }
    public bool IsOccupied { get; private set; }
    public bool IsDirty { get; private set; }

    public GameObject leftoverPrefab;  // Prefab for leftover food
    private GameObject leftoverInstance;  // To hold the instantiated leftover food object

    // Set table number and capacity
    public void InitializeTable(int tableNumber, int capacity)
    {
        TableNumber = tableNumber;
        Capacity = capacity;
        IsOccupied = false; // Set table as unoccupied initially
        IsDirty = false;    // Set table as clean initially
    }

    // Occupy and vacate methods for table status
    public void Occupy() => IsOccupied = true;
    public void Vacate()
    {
        IsOccupied = false;
        SetDirty();  // After customer leaves, make the table dirty and show leftovers
    }


    // Set the table as dirty and spawn the leftover prefab
    public void SetDirty()
    {
        IsDirty = true;
        if (leftoverPrefab != null && leftoverInstance == null)
        {
            leftoverInstance = Instantiate(leftoverPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }


    // Clean the table, destroying the leftovers prefab and marking it clean
    public void Clean()
    {
        IsDirty = false;
        if (leftoverInstance != null)
        {
            Destroy(leftoverInstance);  // Destroy the leftover prefab when cleaning
            leftoverInstance = null;    // Ensure it's not reused
        }
    }
}
