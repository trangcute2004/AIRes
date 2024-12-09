using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum State { LookingForTable, WaitingStaffToCome, GiveOrderToWaitStaff, OrderGiven, WaitingForFood, Eating, Leaving }
    public State currentState = State.LookingForTable;
    public string CurrentState => currentState.ToString();

    public Table assignedTable;
    private Order currentOrder;
    private float eatingTimer;
    private List<Order> menu = new List<Order>();
    private GameObject dishInstance;

    private bool hasGivenOrder = false;
    public bool HasGivenOrder => hasGivenOrder;
    public GameObject doorPrefab;
    private Vector3 doorPosition;

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
        if (assignedTable != null)
        {
            Debug.Log($"Customer {gameObject.name} already has a table assigned.");
            return;  // Don't reassign the table if already assigned
        }

        // Log all the tables with their statuses
        foreach (var table in tables)
        {
            Debug.Log($"Table {table.TableNumber} - Occupied: {table.IsOccupied}, Dirty: {table.IsDirty}");
        }

        // Find a table that is not occupied and is clean
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty);


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


    private void MoveToTable()
    {
        if (assignedTable == null) return;

        Debug.Log("Moving to table...");  // Debug message to confirm it's executing
        float step = 3f * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, assignedTable.transform.position, step);

        if (Vector3.Distance(transform.position, assignedTable.transform.position) < 0.1f)
        {
            Debug.Log($"Customer reached table: {assignedTable.TableNumber}");
            currentState = State.WaitingStaffToCome;  // Transition to waiting for staff once the customer reaches the table
        }
    }

    private void ShowRandomOrder()
    {
        if (menu == null || menu.Count == 0)
        {
            Debug.LogError($"Menu is empty for Customer {gameObject.name}. Ensure the menu is set in the GameController.");
            return;
        }

        int randomIndex = Random.Range(0, menu.Count);
        currentOrder = menu[randomIndex];

        if (currentOrder?.DishPrefab == null)
        {
            Debug.LogError($"Selected order is null or its prefab is missing for Customer {gameObject.name}. Check menu setup.");
            return;
        }

        Debug.Log($"Customer {gameObject.name} selected: {currentOrder.DishName}");

        if (assignedTable != null)
        {
            Vector3 dishPosition = assignedTable.transform.position + Vector3.up * 1.5f;
            dishInstance = Instantiate(currentOrder.DishPrefab, dishPosition, Quaternion.identity);
        }
    }

    public Order GiveOrderToWaitStaff()
    {
        if (currentState != State.GiveOrderToWaitStaff)
        {
            Debug.LogWarning($"Customer {gameObject.name} is not ready to give an order. State: {currentState}, Order: {(currentOrder == null ? "None" : currentOrder.DishName)}");
            return null;
        }

        if (currentOrder == null)
        {
            Debug.LogWarning($"Customer {gameObject.name} has no order selected.");
            return null;
        }

        Debug.Log($"Customer {gameObject.name} is giving order: {currentOrder.DishName}.");
        Order orderToReturn = currentOrder;
        return orderToReturn;
    }

    public void OnOrderTakenByWaitStaff()
    {
        if (currentState == State.GiveOrderToWaitStaff)
        {
            Debug.Log($"Customer {gameObject.name} has given the order.");
            currentState = State.OrderGiven;
        }

        if (currentState == State.OrderGiven)
        {
            Debug.Log($"Customer {gameObject.name} is now waiting for food.");
            currentState = State.WaitingForFood; // Correct state transition
        }
    }


    public void OnOrderDelivered()
    {
        Debug.Log($"Customer {gameObject.name} is now eating.");
        currentState = State.Eating;
        StartEating();
    }

    public void NotifyWaitStaffArrived()
    {
        Debug.Log($"WaitStaff has arrived for Customer {gameObject.name}. Current State: {currentState}");

        if (currentState == State.WaitingStaffToCome && !hasGivenOrder)
        {
            if (currentOrder == null)
            {
                ShowRandomOrder(); // Ensure the customer selects an order first
            }

            if (currentOrder != null)
            {
                currentState = State.GiveOrderToWaitStaff;
                GameController.instance.WaitStaff.AddCustomerToQueue(this);
                Debug.Log($"Customer {gameObject.name}: Now ready to give order.");
            }
            else
            {
                Debug.LogWarning($"Customer {gameObject.name} has no order selected.");
            }
        }
    }


    private void WaitForOrder()
    {
        if (currentOrder == null)
        {
            Debug.LogError("No order to wait for.");
            return;
        }

        // Check if food has been delivered
        if (currentOrder.IsDelivered)
        {
            currentState = State.Eating;  // Transition to Eating state after food is delivered
            StartEating();  // Start eating once food is delivered
        }
        else
        {
            Debug.Log($"Customer {gameObject.name} is still waiting for their food.");
        }
    }


    private void StartEating()
    {
        if (currentOrder != null)
        {
            eatingTimer = currentOrder.EatingDuration;
        }

        currentState = State.Eating;

        if (dishInstance != null)
        {
            dishInstance.SetActive(true);
        }
    }

    private void Eat()
    {
        eatingTimer -= Time.deltaTime;

        if (eatingTimer <= 0f)
        {
            currentState = State.Leaving; // Transition to Leaving once the eating is done

            if (dishInstance != null)
            {
                Destroy(dishInstance);
            }

            if (assignedTable != null)
            {
                assignedTable.Vacate();  // Mark the table dirty and show leftovers
                assignedTable.IsDirty = true; // Set the table as dirty
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
            // Step 1: Vacate the table and set it dirty
            if (assignedTable != null)
            {
                assignedTable.Vacate();  // Mark the table dirty
                assignedTable.SetDirty();  // Ensure it's marked dirty
            }

            if (dishInstance != null)
            {
                Destroy(dishInstance);  // Remove the food dish instance
            }

            Destroy(gameObject);  // Remove the customer object
        }

        // Step 2: Clean the table after the customer leaves
        if (assignedTable != null && assignedTable.IsDirty)
        {
            assignedTable.Clean();  // Clean the table after the customer leaves
            assignedTable.IsDirty = false; // Set the table back to clean
        }
    }

}
