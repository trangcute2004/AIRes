using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingForOrder, Eating, Leaving }
    private State currentState = State.LookingForTable;
    private Table assignedTable;

    // Eating time before customer leaves
    private float eatingDuration = 5f; // Duration in seconds
    private float eatingTimer;

    // This method finds an empty, clean table and assigns it to the customer
    public void FindTable(List<Table> tables)
    {
        // Find an available table that is neither occupied nor dirty
        assignedTable = tables.Find(t => t != null && !t.IsOccupied && !t.IsDirty);

        // If a table is found, occupy it and update the customer state
        if (assignedTable != null)
        {
            assignedTable.Occupy(); // Mark the table as occupied
            transform.position = assignedTable.transform.position; // Move the customer to the table position
            currentState = State.WaitingForOrder; // Change state to WaitingForOrder
            Debug.Log($"Customer assigned to Table {assignedTable.TableNumber}");
        }
        else
        {
            // If no table is available, log a message
            Debug.Log("No available tables for the customer.");

            // Optional behavior: Customers might wait or leave if no table is found
            currentState = State.Leaving; // Set the state to Leaving (optional, adjust logic as needed)
                                          // You could also introduce a timeout or a waiting mechanism before they leave or keep looking for a table
        }
    }

    // UpdateState is called each frame to handle the customer's state transitions
    public void UpdateState()
    {
        switch (currentState)
        {
            case State.LookingForTable:
                // FindTable handles this state
                break;

            case State.WaitingForOrder:
                // Waiting for wait staff to bring the order, add wait time or interaction here if needed
                break;

            case State.Eating:
                Eat(); // Call Eat method to manage eating duration
                break;

            case State.Leaving:
                Leave(); // Call Leave method when ready to exit
                break;
        }
    }

    // Eat method simulates the eating duration with a timer
    private void Eat()
    {
        eatingTimer -= Time.deltaTime;

        if (eatingTimer <= 0f)
        {
            currentState = State.Leaving; // Move to Leaving state when eating is done
        }
    }

    // Leave method vacates the table and destroys the customer object
    private void Leave()
    {
        if (assignedTable != null)
        {
            assignedTable.Vacate(); // Free up the table for other customers
        }

        Destroy(gameObject); // Remove the customer from the scene
    }

    // Method to start eating and set the eating timer
    public void StartEating()
    {
        eatingTimer = eatingDuration;
        currentState = State.Eating; // Switch to Eating state
    }
}
