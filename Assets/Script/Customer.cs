using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingForOrder, Eating, Leaving }
    private State currentState = State.LookingForTable;
    private Table assignedTable;
    private Order currentOrder;

    // Eating time before customer leaves
    private float eatingDuration = 5f; // Duration in seconds
    private float eatingTimer;

    // List of food items available to order
    private List<Order> menu = new List<Order>();

    public void SetMenu(List<Order> availableMenu)
    {
        menu = availableMenu;
    }

    // This method finds an empty, clean table and assigns it to the customer
    public void FindTable(List<Table> tables)
    {
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty); // Look for an available table

        if (assignedTable != null)
        {
            assignedTable.Occupy(); // Mark the table as occupied
            transform.position = assignedTable.transform.position; // Move customer to the table position
            currentState = State.WaitingForOrder; // Change state to WaitingForOrder
        }
        else
        {
            Debug.Log("No available tables for the customer.");
            // Optionally: Add behavior for customers who wait for a table or leave if none is found
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

    // Waiting for order to be served
    private void WaitForOrder()
    {
        if (currentOrder == null)
        {
            // Simulate ordering by selecting a random order (dish) from the menu
            currentOrder = menu[Random.Range(0, menu.Count)];
            Debug.Log($"Customer ordered: {currentOrder.DishName} (Preparation Time: {currentOrder.GetPreparationTime()}s)");

            // Start the timer for the order's preparation time
            currentState = State.Eating; // Transition to eating after the order is placed
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
