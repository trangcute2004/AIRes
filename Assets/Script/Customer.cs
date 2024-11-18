using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingForOrder, WaitingForFood, Eating, Leaving }
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
    public WaitStaff assignedWaitStaff;

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

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.LookingForTable:
                MoveToTable();
                break;
            case State.WaitingForOrder:
                if (assignedWaitStaff != null && assignedWaitStaff.IsIdle())
                {
                    assignedWaitStaff.SetTargetCustomer((this));
                }
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
            // Immediately select a random dish
            Debug.Log($"Customer {gameObject.name} reached table and is now selecting a dish.");
            ShowRandomOrder();

            // Transition to WaitingForOrder state
            currentState = State.WaitingForOrder;
        }
    }

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

        // Instantiate the dish
        if (assignedTable != null)
        {
            Vector3 dishPosition = assignedTable.transform.position + Vector3.up * 1.5f; // Adjust height if needed
            dishInstance = Instantiate(currentOrder.DishPrefab, dishPosition, Quaternion.identity);
        }
    }


    public Order GiveOrderToWaitStaff()
    {
        if (currentState != State.WaitingForOrder || currentOrder == null)
        {
            Debug.LogWarning($"Customer is not ready to give an order. State: {currentState}, Order: {(currentOrder == null ? "None" : currentOrder.DishName)}");
            return null;
        }

        Debug.Log($"Customer is giving order: {currentOrder.DishName}");
        currentState = State.WaitingForFood;
        return currentOrder;
    }

    public bool IsReadyToOrder()
    {
        bool ready = currentState == State.WaitingForOrder && currentOrder != null;
        Debug.Log($"IsReadyToOrder: {ready}, CurrentState: {currentState}, HasOrder: {currentOrder != null}");
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


    public void SetAssignedWaitStaff(WaitStaff waitStaff)
    {
        assignedWaitStaff = waitStaff;
        Debug.Log($"Assigned WaitStaff {waitStaff.gameObject.name} to Customer {gameObject.name}");
    }
}
