using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingStaffToCome, GiveOrderToStaff, WaitingForFood, Eating, Leaving }
    private State currentState = State.LookingForTable;
    public string CurrentState => currentState.ToString();

    private Table assignedTable;
    private Order currentOrder;
    private float eatingDuration = 5f;
    private float eatingTimer;
    private List<Order> menu = new List<Order>();
    private GameObject dishInstance;

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

            case State.GiveOrderToStaff:
                NotifyWaitStaffArrived();
                // When waitstaff reaches the customer, give the order
                Debug.Log($"Customer {gameObject.name} is ready to give the order to the waitstaff.");
                GiveOrderToWaitStaff();
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
        Debug.Log($"Current State: {currentState}, Current Order: {(currentOrder == null ? "None" : currentOrder.DishName)}");

        if (currentState != State.GiveOrderToStaff || currentOrder == null)
        {
            Debug.LogWarning($"Customer {gameObject.name} is not ready to give an order. State: {currentState}, Order: {(currentOrder == null ? "None" : currentOrder.DishName)}");
            return null;
        }

        Debug.Log($"Customer {gameObject.name} is giving order: {currentOrder.DishName}.");
        currentState = State.WaitingForFood; // Transition to waiting for food
        return currentOrder;
    }



    public void NotifyWaitStaffArrived()
    {
        Debug.Log($"WaitStaff has arrived for Customer {gameObject.name}. Current State: {currentState}");
        if (currentState == State.WaitingStaffToCome)
        {
            Debug.Log($"Customer {gameObject.name}: Ready to give the order.");
            currentState = State.GiveOrderToStaff;
            ShowRandomOrder();
        }
    }


    public bool IsReadyToOrder()
    {
        bool ready = currentState == State.WaitingStaffToCome && currentOrder != null;
        //Debug.Log($"IsReadyToOrder: {ready}, CurrentState: {currentState}, HasOrder: {currentOrder != null}");
        return ready;
    }

    private void WaitForOrder()
    {
        StartEating();
    }

    private void StartEating()
    {
        eatingTimer = eatingDuration;
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
            currentState = State.Leaving;

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
