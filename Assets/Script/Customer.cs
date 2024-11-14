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
        menu = availableMenu;
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
            currentState = State.WaitingForOrder;
            ShowRandomOrder();
        }
    }

    public void ShowRandomOrder()
    {
        if (menu.Count == 0)
        {
            Debug.LogError("Menu is empty!");
            return;
        }

        currentOrder = menu[Random.Range(0, menu.Count)];
        Debug.Log($"Customer {gameObject.name} has chosen: {currentOrder.DishName}");

        if (currentOrder.DishPrefab != null)
        {
            dishInstance = Instantiate(currentOrder.DishPrefab, assignedTable.transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Dish prefab for {currentOrder.DishName} is missing!");
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
        return currentState == State.WaitingForOrder && currentOrder != null;
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
            Destroy(gameObject);
        }
    }

    public void SetAssignedWaitStaff(WaitStaff waitStaff)
    {
        assignedWaitStaff = waitStaff;
        Debug.Log($"Assigned WaitStaff {waitStaff.gameObject.name} to Customer {gameObject.name}");
    }
}
