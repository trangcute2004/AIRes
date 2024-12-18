using Unity.VisualScripting;
using UnityEngine;

public class Table : MonoBehaviour
{
    //represent the table number
    public int TableNumber { get; private set; }

    //represent the capacity of the table
    public int Capacity { get; private set; }

    //checks if the table is currently occupied by a customer or not.
    public bool IsOccupied { get; private set; }

    //check if the table is dirty
    public bool IsDirty { get; set; } 

    public GameObject leftoverPrefab;

    //store the instance of the leftover object
    private GameObject leftoverInstance;

    //initialize the properties of a table
    public void InitializeTable(int tableNumber, int capacity)
    {
        // Set the number for table
        TableNumber = tableNumber;
        //Set the seating capacity of the table
        Capacity = capacity;
        IsOccupied = false; // The table is not occupied at the start.
        IsDirty = false; // The table is clean at the start.
    }

    //marks the table as occupied when a customer sits down at it
    public void Occupy()
    {
        //// Check if the table is already occupied.
        if (IsOccupied)
        {
            Debug.LogWarning("Table is already occupied!");
            return;
        }

        // // If the table is not occupied, we mark it as occupied.
        IsOccupied = true;
        Debug.Log("Table has been occupied.");
    }

    //when a customer leaves the table, making it available for new customers
    public void Vacate()
    {
        // Mark the table as unoccupied by setting IsOccupied to false.
        IsOccupied = false;

        //the table is now dirty (needs cleaning).
        SetDirty();
    }

    //marks the table as dirty
    public void SetDirty()
    {
        IsDirty = true;  // This marks the table as dirty.

        // Check if the leftoverPrefab is assigned and if no leftover instance exists already.
        if (leftoverPrefab != null && leftoverInstance == null)
        {
            // If there isn't already a leftover instance, create one at the table's position
            leftoverInstance = Instantiate(leftoverPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    //clean the table and mark it as ready for new customers
    public void Clean()
    {
        // Check if the table is occupied before cleaning it.
        if (IsOccupied)
        {
            Debug.LogWarning("Cleaning an occupied table!");
            return;
        }

        //Mark the table as clean
        IsDirty = false;  // Set the table to clean
        Debug.Log("Table has been cleaned.");
    }
}
