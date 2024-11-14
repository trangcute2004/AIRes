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

        foreach (var order in availableMenu)
        {
            if (order == null || order.DishPrefab == null)
            {
                Debug.LogError($"Invalid order detected in the menu for Customer {gameObject.name}: {order?.DishName ?? "null"}");
            }
            else
            {
                Debug.Log($"Valid order for Customer {gameObject.name}: {order.DishName}, Prefab: {order.DishPrefab.name}");
            }
        }

        menu = availableMenu;
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
                    assignedWaitStaff.TakeOrderFromCustomer();
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

        if (Vector3.Distance(transform.position, assignedTable.transform.position) < 0.1f)
        {
            currentState = State.WaitingForOrder; // Transition to WaitingForOrder
            Debug.Log($"Customer {gameObject.name} reached table and is now WaitingForOrder.");
            ShowRandomOrder();
        }
    }


    private void ShowRandomOrder()
    {
        if (menu == null || menu.Count == 0)
        {
            Debug.LogError($"Customer {gameObject.name} received an empty or uninitialized menu. Ensure the menu is properly set.");
            return;
        }

        Debug.Log($"Customer {gameObject.name} selecting a random dish from the menu:");

        foreach (var order in menu)
        {
            Debug.Log($" - Dish: {order?.DishName ?? "null"}");
        }

        // Select a random order
        Order selectedOrder = menu[Random.Range(0, menu.Count)];
        if (selectedOrder == null || selectedOrder.DishPrefab == null)
        {
            Debug.LogError($"Failed to select a valid order for Customer {gameObject.name}. Ensure the menu contains valid orders with dish prefabs.");
            return;
        }

        // Assign the selected order to the customer
        currentOrder = new Order(selectedOrder.DishName, selectedOrder.PreparationTime, selectedOrder.DishPrefab);
        Debug.Log($"Customer {gameObject.name} has selected: {currentOrder.DishName}");

        // Instantiate the dish prefab at the table
        if (assignedTable != null)
        {
            Vector3 dishPosition = assignedTable.transform.position + new Vector3(0, 1, 0); // Slightly above the table
            dishInstance = Instantiate(currentOrder.DishPrefab, dishPosition, Quaternion.identity);
            Debug.Log($"Dish {currentOrder.DishName} has been instantiated at the table for Customer {gameObject.name}.");
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
