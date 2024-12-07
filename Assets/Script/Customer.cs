using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum State { LookingForTable, WaitingStaffToCome, GiveOrderToWaitStaff, OrderGiven, WaitingForFood, Eating, Leaving }
    public State currentState = State.LookingForTable;
    public string CurrentState => currentState.ToString();

    private Table assignedTable;
    private Order currentOrder;
    //private float eatingDuration = 5f;
    private float eatingTimer;
    private List<Order> menu = new List<Order>();
    private GameObject dishInstance;

    private bool hasGivenOrder = false; // Tracks if the order has already been given
    public bool HasGivenOrder => hasGivenOrder; // Public read-only property
    public GameObject doorPrefab;
    private Vector3 doorPosition;
    //public WaitStaff assignedWaitStaff;



    public void SetMenu(List<Order> availableMenu)
    {
        if (availableMenu == null || availableMenu.Count == 0)
        {
            Debug.LogError($"Customer {gameObject.name} received an empty or null menu.");
            return;
        }

        menu = new List<Order>(availableMenu);
        Debug.Log($"Customer {gameObject.name} received a menu with {menu.Count} items.");
    }


    private void Start()
    {
        if (doorPrefab == null)
        {
            Debug.LogError("Door prefab is not assigned! Please assign it in the Inspector.");
            return;
        }

        doorPosition = doorPrefab.transform.position;
    }

    private void Update()
    {
        UpdateState();
    }

    private State previousState;

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.LookingForTable:
                MoveToTable();
                break;

            case State.WaitingStaffToCome:
                if (currentState == previousState)
                    return;

                previousState = currentState;
                if (GameController.instance.WaitStaff.IsIdle())
                {
                    Debug.Log("Attempting to assign WaitStaff to Customer.");
                    GameController.instance.WaitStaff.AddCustomerToQueue(this);
                    Debug.Log("AddCustomerToQueue method executed.");
                }
                break;

            case State.GiveOrderToWaitStaff:
                NotifyWaitStaffArrived();
                // When waitstaff reaches the customer, give the order
                //Debug.Log($"Customer {gameObject.name} is ready to give the order to the waitstaff.");
                //GiveOrderToWaitStaff();
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

    public void FindTable(List<Table> tables)
    {
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty);

        if (assignedTable != null)
        {
            assignedTable.Occupy();
            currentState = State.LookingForTable;
            previousState=currentState;
        }
        else
        {
            Debug.Log("No available tables for the customer.");
        }
    }

    private void MoveToTable()
    {
        if (assignedTable == null) return;

        float step = 3f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, assignedTable.transform.position, step);

        // Check if the customer has reached the table
        if (Vector3.Distance(transform.position, assignedTable.transform.position) < 0.1f)
        {
            Debug.Log($"Customer {gameObject.name} reached table and is now waiting for the waiter.");
            currentState = State.WaitingStaffToCome;  // Transition to Waiting for Waiter
        }
    }

    // Show a random order from the menu
    private void ShowRandomOrder()
    {
        if (menu == null || menu.Count == 0)
        {
            Debug.LogError($"Menu is empty for Customer {gameObject.name}. Ensure the menu is set in the GameController.");
            return;
        }

        // Random selection
        int randomIndex = Random.Range(0, menu.Count);
        currentOrder = menu[randomIndex];

        if (currentOrder?.DishPrefab == null)
        {
            Debug.LogError($"Selected order is null or its prefab is missing for Customer {gameObject.name}. Check menu setup.");
            return;
        }

        Debug.Log($"Customer {gameObject.name} selected: {currentOrder.DishName}");

        // Instantiate the dish prefab at the table position
        if (assignedTable != null)
        {
            Vector3 dishPosition = assignedTable.transform.position + Vector3.up * 1.5f; // Adjust height if needed
            dishInstance = Instantiate(currentOrder.DishPrefab, dishPosition, Quaternion.identity);
        }
    }


    public Order GiveOrderToWaitStaff()
    {
        // Ensure the customer is in the correct state to give the order
        if (currentState != State.GiveOrderToWaitStaff)
        {
            Debug.LogWarning($"Customer {gameObject.name} is not ready to give an order. State: {currentState}, Order: {(currentOrder == null ? "None" : currentOrder.DishName)}");
            return null;
        }

        // Check if the customer has an order to give
        if (currentOrder == null)
        {
            Debug.LogWarning($"Customer {gameObject.name} has no order selected.");
            return null;
        }

        Debug.Log($"Customer {gameObject.name} is giving order: {currentOrder.DishName}.");

        // Return the order (don't transition yet to 'OrderGiven')
        Order orderToReturn = currentOrder;

        // Do not change state to OrderGiven here; WaitStaff will change it when the order is taken
        return orderToReturn;
    }

    public void OnOrderTakenByWaitStaff()
    {
        // Transition to 'OrderGiven' after WaitStaff has taken the order
        if (currentState == State.GiveOrderToWaitStaff)
        {
            Debug.Log($"Customer {gameObject.name} has given the order.");
            currentState = State.OrderGiven;  // Transition to OrderGiven state after order is given
        }

        // Transition to "WaitingForFood" after order is placed
        if (currentState == State.OrderGiven)
        {
            Debug.Log($"Customer {gameObject.name} is waiting for food.");
            currentState = State.WaitingForFood; // Transition to waiting for food
        }
    }

    public void OnOrderDelivered()
    {
        // Once the order is delivered, transition to Eating state
        Debug.Log($"Customer {gameObject.name} is now eating.");
        currentState = State.Eating;  // Transition to eating state

        // Start the eating timer based on the food's EatingDuration
        StartEating();  // Start eating with the appropriate eating duration
    }


    public void NotifyWaitStaffArrived()
    {
        Debug.Log($"WaitStaff has arrived for Customer {gameObject.name}. Current State: {currentState}");

        // Ensure customer has selected an order before transitioning
        if (currentState == State.WaitingStaffToCome && !hasGivenOrder)
        {
            if (currentOrder == null)
            {
                ShowRandomOrder();  // Select a random order for the customer (or provide a mechanism to choose)
            }

            if (currentOrder != null)
            {
                currentState = State.GiveOrderToWaitStaff;  // Customer is now ready to give the order
                GameController.instance.WaitStaff.AddCustomerToQueue(this); // Add customer to the queue
                Debug.Log($"Customer {gameObject.name}: Now ready to give order.");
            }
            else
            {
                Debug.LogWarning($"Customer {gameObject.name} has no order selected.");
            }
        }
    }


    public void SetToReadyForOrder()
    {
        if (currentState == State.WaitingForFood)
        {
            Debug.Log($"Customer {gameObject.name} is now ready to give the order.");
            currentState = State.GiveOrderToWaitStaff;  // Transition to giving the order
        }
    }

    public bool IsReadyToOrder()
    {
        return currentState == State.GiveOrderToWaitStaff && currentOrder != null;
    }

    private void WaitForOrder()
    {
        if (currentOrder == null)
        {
            Debug.LogError("No order to wait for.");
            return;
        }

        // Check if the food has been delivered by the WaitStaff
        if (currentState == State.WaitingForFood && currentOrder.IsDelivered)
        {
            // Transition to eating state once food is delivered
            Debug.Log($"Food delivered to Customer {gameObject.name}. Starting to eat.");
            StartEating();  // Begin eating with the correct time
        }
        else
        {
            // Continue to wait for the food
            Debug.Log($"Customer {gameObject.name} is still waiting for their food.");
        }
    }


    private void StartEating()
    {
        // Use the specific eating duration for the dish that was ordered
        if (currentOrder != null)
        {
            eatingTimer = currentOrder.EatingDuration;  // Set the eating timer based on the dish's eating duration
        }

        currentState = State.Eating;  // Transition to eating state

        // Optionally, activate the dish instance if it's not already active
        if (dishInstance != null)
        {
            dishInstance.SetActive(true);
        }
    }


    private void Eat()
    {
        // Decrease the eating timer over time
        eatingTimer -= Time.deltaTime;

        if (eatingTimer <= 0f)
        {
            currentState = State.Leaving;  // Transition to leaving state when done eating

            // Destroy the dish instance when the customer is done eating
            if (dishInstance != null)
            {
                Destroy(dishInstance);
            }
        }
    }

    private void MoveToDoor()
    {
        if (doorPosition == Vector3.zero)
        {
            Debug.LogError("Door position is not set. Ensure doorPrefab is assigned correctly.");
            return;
        }

        float step = 3f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, doorPosition, step);

        if (Vector3.Distance(transform.position, doorPosition) < 0.1f)
        {
            if (assignedTable != null)
            {
                assignedTable.Vacate();
            }

            // Destroy dish instance when customer leaves
            if (dishInstance != null)
            {
                Destroy(dishInstance);
            }

            Destroy(gameObject);
        }
    }
}
