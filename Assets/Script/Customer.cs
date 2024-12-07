using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum State { LookingForTable, WaitingStaffToCome, GiveOrderToWaitStaff, OrderGiven, WaitingForFood, Eating, Leaving }
    public State currentState = State.LookingForTable;
    public string CurrentState => currentState.ToString();

    private Table assignedTable;
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
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty);
        if (assignedTable != null)
        {
            assignedTable.Occupy();
            currentState = State.LookingForTable;
            previousState = currentState;
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
            Debug.Log($"Customer {gameObject.name} reached table and is now waiting for the waiter.");
            currentState = State.WaitingStaffToCome;  // Transition to Waiting for Waiter
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
            Debug.Log($"Customer {gameObject.name} is waiting for food.");
            currentState = State.WaitingForFood;
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
                ShowRandomOrder();
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

        if (currentState == State.WaitingForFood && currentOrder.IsDelivered)
        {
            Debug.Log($"Food delivered to Customer {gameObject.name}. Starting to eat.");
            StartEating();
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

            if (dishInstance != null)
            {
                Destroy(dishInstance);
            }

            Destroy(gameObject);
        }
    }
}
