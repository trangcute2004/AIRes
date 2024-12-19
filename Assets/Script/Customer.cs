using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Customer : MonoBehaviour
{
    //represent different states a customer can be
    public enum State { LookingForTable, WaitingStaffToCome, GiveOrderToWaitStaff, OrderGiven, WaitingForFood, Eating, Leaving }

    // The current state of the customer, initialized to "LookingForTable" when the script starts.
    public State currentState = State.LookingForTable;

    //for display on console
    public string CurrentState => currentState.ToString();

    // The table assigned to the customer once they are seated
    public Table assignedTable;

    // The current order that the customer has placed
    private Order currentOrder;

    // A timer to track how long the customer has been eating their food
    private float eatingTimer;

    // A list to hold the menu items (orders)
    private List<Order> menu = new List<Order>();

    //The instance of the dish
    private GameObject dishInstance;

    //check if the customer has already given their order or not.
    private bool hasGivenOrder = false;

    //for display on console
    public bool HasGivenOrder => hasGivenOrder;


    public GameObject doorPrefab;
    //The position where the door will be placed in the scene
    private Vector3 doorPosition;

    private void Start()
    {
        // Check if the doorPrefab is not assigned in the Inspector
        if (doorPrefab == null)
        {
            Debug.LogError("Door prefab is not assigned! Please assign it in the Inspector.");
            return;
        }
        // If the doorPrefab is assigned, store its position in the doorPosition variable
        doorPosition = doorPrefab.transform.position;
    }

    private void Update()
    {
        UpdateState();
    }

    //store the previous state of the customer
    private State previousState;


    //FINITE STATE MACHINE
    private void UpdateState()
    {
        switch (currentState)
        {
            case State.LookingForTable:
                MoveToTable();
                break;

            case State.WaitingStaffToCome:
                // If the state hasn't changed from the previous one, do nothing
                if (currentState == previousState)
                    return;

                // Update the previous state to the current one
                previousState = currentState;

                // If the wait staff is idle, attempt to assign them to the customer
                if (GameController.instance.WaitStaff.IsIdle())
                {
                    Debug.Log("Attempting to assign WaitStaff to Customer.");
                    // Add the customer to the wait staff's queue.
                    GameController.instance.WaitStaff.AddCustomerToQueue(this);
                    Debug.Log("AddCustomerToQueue method executed.");
                }
                break;

            case State.GiveOrderToWaitStaff:
                NotifyWaitStaffArrived();
                break;

            case State.OrderGiven:
                break;

            case State.WaitingForFood:
                WaitForOrder();
                break;

            case State.Eating:
                Eat();
                break;

            case State.Leaving:
                MoveToDoor();
                break;
        }
    }

    //find a suitable table for the customer
    public void FindTable(List<Table> tables)
    {

        // If the customer already has an assigned table
        if (assignedTable != null)
        {
            Debug.Log($"Customer {gameObject.name} already has a table assigned.");
            return;
        }

        // each table's current status (if it's occupied or dirty)
        foreach (var table in tables)
        {
            Debug.Log($"Table {table.TableNumber} - Occupied: {table.IsOccupied}, Dirty: {table.IsDirty}");
        }

        // Find a table that is not occupied and is clean
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty);

        // If a suitable table is found, occupy the table and do the action
        if (assignedTable != null)
        {
            assignedTable.Occupy();  // Mark the table as occupied
            Debug.Log($"Customer {gameObject.name} found table {assignedTable.TableNumber}");
            currentState = State.LookingForTable;  // Transition to looking for table state
        }
        else
        {
            Debug.Log($"Customer {gameObject.name} could not find an available table.");
        }
    }

    //PATHFINDING
    //moves the customer to the assigned table and handles the state transition once the customer reaches the table
    private void MoveToTable()
    {
        // If the customer does not have an assigned table
        if (assignedTable == null) return;
        Debug.Log("Moving to table...");  // Debug message to confirm it's executing

        //the step size of customer for movement
        float step = 3f * Time.deltaTime;

        // Move the customer towards the table's position
        transform.position = Vector3.MoveTowards(transform.position, assignedTable.transform.position, step);

        // Check if the customer has reached the table
        if (Vector3.Distance(transform.position, assignedTable.transform.position) < 0.1f)
        {
            Debug.Log($"Customer reached table: {assignedTable.TableNumber}");
            currentState = State.WaitingStaffToCome;  // Transition to waiting for staff once the customer reaches the table
        }
    }

    //set the menu for the customer
    public void SetMenu(List<Order> availableMenu)
    {
        // Check if the available menu is null or empty
        if (availableMenu == null || availableMenu.Count == 0)
        {
            Debug.LogError($"Customer {gameObject.name} received an empty or null menu.");
            return;
        }

        //If the menu is valid, create a new list and copy the available menu items into it.
        menu = new List<Order>(availableMenu);
        Debug.Log($"Customer {gameObject.name} received a menu with {menu.Count} items.");
    }

    //randomly selects an order from the menu and spawns the dish prefab at the customer's table
    private void ShowRandomOrder()
    {
        // Check if the menu is not empty or null
        if (menu == null || menu.Count == 0)
        {
            Debug.LogError($"Menu is empty for Customer {gameObject.name}. Ensure the menu is set in the GameController.");
            return;
        }
        // Randomly select an index from the menu list.
        int randomIndex = Random.Range(0, menu.Count);

        // Set the selected order to the currentOrder variable
        currentOrder = menu[randomIndex];

        // Check if the selected order's dish prefab is missing or null.
        if (currentOrder?.DishPrefab == null)
        {
            Debug.LogError($"Selected order is null or its prefab is missing for Customer {gameObject.name}. Check menu setup.");
            return;
        }

        //customer can select the dish
        Debug.Log($"Customer {gameObject.name} selected: {currentOrder.DishName}");

        // If the customer has an assigned table, instantiate the dish prefab at the table's position.
        if (assignedTable != null)
        {
            // Set the dish prefab position slightly above the table
            Vector3 dishPosition = assignedTable.transform.position + Vector3.up * 1.5f;
            // Instantiate the dish prefab at the calculated position and store the reference
            dishInstance = Instantiate(currentOrder.DishPrefab, dishPosition, Quaternion.identity);
        }
    }

    //the wait staff arrives at the customer's table
    public void NotifyWaitStaffArrived()
    {
        Debug.Log($"WaitStaff has arrived for Customer {gameObject.name}. Current State: {currentState}");

        // Check if the customer is in the "WaitingStaffToCome" state and hasn't given their order yet.
        if (currentState == State.WaitingStaffToCome && !hasGivenOrder)
        {
            // Ensure the customer has selected an order before proceeding.
            if (currentOrder == null)
            {
                ShowRandomOrder(); // Ensure the customer selects an order first
            }

            // If the customer now has a valid order, give the order to the wait staff.
            if (currentOrder != null)
            {
                currentState = State.GiveOrderToWaitStaff;
                // Add the customer to the wait staff's queue
                GameController.instance.WaitStaff.AddCustomerToQueue(this);
                Debug.Log($"Customer {gameObject.name}: Now ready to give order.");
            }
            else
            {
                Debug.LogWarning($"Customer {gameObject.name} has no order selected.");
            }
        }
    }

    // customer to give their selected order to the wait staff
    public Order GiveOrderToWaitStaff()
    {
        //if the customer is in the correct state to give an order.
        if (currentState != State.GiveOrderToWaitStaff)
        {
            Debug.LogWarning($"Customer {gameObject.name} is not ready to give an order. State: {currentState}, Order: {(currentOrder == null ? "None" : currentOrder.DishName)}");
            return null;
        }

        //if the customer has selected an order
        if (currentOrder == null)
        {
            Debug.LogWarning($"Customer {gameObject.name} has no order selected.");
            return null;
        }

        //the order being given to the wait staff
        Debug.Log($"Customer {gameObject.name} is giving order: {currentOrder.DishName}.");

        // Return the current order to be processed by the wait staff
        Order orderToReturn = currentOrder;
        return orderToReturn;
    }

    //the wait staff takes the customer's order
    public void OnOrderTakenByWaitStaff()
    {
        //the customer has just given their order if the curent state is correct
        if (currentState == State.GiveOrderToWaitStaff)
        {
            Debug.Log($"Customer {gameObject.name} has given the order.");
            // the order has been taken
            currentState = State.OrderGiven;
        }

        // if the current state is "OrderGiven", the order has been accepted
        if (currentState == State.OrderGiven)
        {
            Debug.Log($"Customer {gameObject.name} is now waiting for food.");
            //Transition to the state that the customer waits for their food
            currentState = State.WaitingForFood;
        }
    }

    //the customer is waiting for their food to be delivered
    private void WaitForOrder()
    {
        /// Check if the customer has an order
        if (currentOrder == null)
        {
            Debug.LogError("No order to wait for.");
            return;
        }

        // Check if food has been delivered
        if (currentOrder.IsDelivered)
        {
            currentState = State.Eating;  // Transition to Eating state
            StartEating();  // Start eating
        }
        else
        {
            Debug.Log($"Customer {gameObject.name} is still waiting for their food.");
        }
    }

    //the customer's order is delivered.
    public void OnOrderDelivered()
    {
        Debug.Log($"Customer {gameObject.name} is now eating.");
        // Change the customer's state to "Eating"
        currentState = State.Eating;
        StartEating();
    }

    //the customer starts eating their food.
    private void StartEating()
    {
        // Check if the customer has an order
        if (currentOrder != null)
        {
            // Set the eating timer for the current order's eating duration
            eatingTimer = currentOrder.EatingDuration;
        }

        // Change the customer's state to "Eating"
        currentState = State.Eating;


        // Check if the dish instance exists
        if (dishInstance != null)
        {
            // If the dish instance is valid, make it active so it can be seen
            dishInstance.SetActive(true);
        }
    }

    //the customer is eating their food
    private void Eat()
    {
        // Decrease the eating timer
        eatingTimer -= Time.deltaTime;

        // Check if the eating timer has finished
        if (eatingTimer <= 0f)
        {
            currentState = State.Leaving; // Transition to Leaving once the eating is done

            // destroy the food object if the dish instance exists
            if (dishInstance != null)
            {
                Destroy(dishInstance);
            }

            // Check if the customer has an assigned table
            if (assignedTable != null)
            {
                assignedTable.Vacate();  // Mark the table dirty and show leftovers
                assignedTable.IsDirty = true; // Set the table as dirty
            }
        }
    }


    //customer move to door
    private void MoveToDoor()
    {
        // if the door position is valid
        if (doorPosition == Vector3.zero)
        {
            Debug.LogError("Door position is not set. Ensure doorPrefab is assigned correctly.");
            return;
        }

        // the step of customer, they move to the door position
        float step = 3f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, doorPosition, step);

        // the customer is near the door, exit 
        if (Vector3.Distance(transform.position, doorPosition) < 0.1f)
        {
            //If the customer has an assigned table, vacate it
            if (assignedTable != null)
            {
                assignedTable.Vacate();  // Mark the table dirty
                assignedTable.SetDirty();  // Ensure it's marked dirty
            }

            //If the customer has a dish, destroy it
            if (dishInstance != null)
            {
                Destroy(dishInstance);  // Remove the food dish instance
            }

            Destroy(gameObject);  // Remove the customer object
        }

        //After the customer leaves, if the table is dirty, the wait staff have to clean it
        if (assignedTable != null && assignedTable.IsDirty)
        {
            assignedTable.Clean();  // Clean the table after the customer leaves
            assignedTable.IsDirty = false; // Set the table back to clean
        }
    }

}
