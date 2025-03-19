using Unity.VisualScripting;
using UnityEngine;

public class Table : MonoBehaviour
{
    public int TableNumber { get; private set; }
    public int Capacity { get; private set; }

    public bool IsOccupied = false;  // Whether the table is currently occupied
    public bool IsDirty = false;  // Whether the table is dirty after use

    public GameObject leftoverPrefab;  // The prefab of the leftover object (food remains, etc.)
    private GameObject leftoverInstance;  // Instance of leftover prefab

    // Initialize the properties of a table
    public void InitializeTable(int tableNumber, int capacity)
    {
        TableNumber = tableNumber;
        Capacity = capacity;
        IsOccupied = false;  // The table is not occupied at the start
        IsDirty = false;  // The table is clean at the start
    }

    // Marks the table as occupied when a customer sits down at it
    public void Occupy()
    {
        if (IsOccupied)
        {
            Debug.LogWarning("Table is already occupied!");
            return;
        }

        IsOccupied = true;  // Mark the table as occupied
        Debug.Log("Table has been occupied.");
    }

    // When a customer leaves the table, making it available for new customers
    public void Vacate()
    {
        IsOccupied = false;  // Mark the table as unoccupied
        SetDirty();  // The table becomes dirty after the customer leaves
    }

    // Marks the table as dirty and spawns a leftover prefab
    public void SetDirty()
    {
        IsDirty = true;  // Mark the table as dirty
        Debug.Log($"Table {TableNumber} is now dirty.");

        // Ensure that the leftoverPrefab is assigned and create the leftover instance
        if (leftoverPrefab != null && leftoverInstance == null)
        {
            // Position the leftover prefab slightly above the table (adjust the position if needed)
            Vector3 spawnPosition = transform.position + Vector3.up * 3f;  // 3 units above the table for visibility

            // Instantiate the leftover prefab at the adjusted position
            leftoverInstance = Instantiate(leftoverPrefab, spawnPosition, Quaternion.identity);
            leftoverInstance.transform.parent = transform;  // Optionally set parent to table for better organization
            Debug.Log($"Leftover spawned at position: {spawnPosition}");
        }
        else if (leftoverPrefab == null)
        {
            Debug.LogError("Leftover prefab is not assigned! Please assign it in the Inspector.");
        }
        else
        {
            Debug.LogWarning("Leftover already exists at this table.");
        }
    }


    public void Clean()
    {
        Debug.Log($"Cleaning table {TableNumber}.");
        if (IsOccupied)
        {
            Debug.LogWarning("Cleaning an occupied table!");
            return;
        }

        IsDirty = false;  // Set the table to clean
        Debug.Log("Table has been cleaned.");

        // Destroy the leftover instance if it exists and reset it
        if (leftoverInstance != null)
        {
            Debug.Log("Destroying leftover object.");
            Destroy(leftoverInstance);  // Remove the leftover object
            leftoverInstance = null;  // Reset the leftover instance reference
        }
    }

}
