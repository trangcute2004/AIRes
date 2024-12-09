using UnityEngine;

public class Table : MonoBehaviour
{
    public int TableNumber { get; private set; }
    public int Capacity { get; private set; }
    public bool IsOccupied { get; private set; }
    public bool IsDirty { get; set; }  // Keep it private but create setter methods

    public GameObject leftoverPrefab;
    private GameObject leftoverInstance;

    public void InitializeTable(int tableNumber, int capacity)
    {
        TableNumber = tableNumber;
        Capacity = capacity;
        IsOccupied = false;
        IsDirty = false;
    }

    public void Occupy()
    {
        if (IsOccupied)
        {
            Debug.LogWarning("Table is already occupied!");
            return;
        }
        IsOccupied = true;
        Debug.Log("Table has been occupied.");
    }

    public void Vacate()
    {
        IsOccupied = false;
        SetDirty();
    }

    public void SetDirty()
    {
        IsDirty = true;  // This marks the table as dirty.
        if (leftoverPrefab != null && leftoverInstance == null)
        {
            leftoverInstance = Instantiate(leftoverPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    public void Clean()
    {
        if (IsOccupied)
        {
            Debug.LogWarning("Cleaning an occupied table!");
            return;
        }

        IsDirty = false;  // Set the table to clean
        Debug.Log("Table has been cleaned.");
    }
}
