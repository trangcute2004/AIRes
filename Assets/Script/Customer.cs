using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingForOrder, WaitingForFood, Eating, Leaving }
    private State currentState = State.LookingForTable;
    private Table assignedTable;
    private Order currentOrder;

    private float eatingDuration = 5f;
    private float eatingTimer;

    private List<Order> menu = new List<Order>();

    public GameObject orderPrefab; // Drag Image prefab here (if using images)
    private GameObject orderInstance;

    public Vector3 spawnPosition; // Position where the customer spawns (e.g., a corner)
    public GameObject doorPrefab;  // Drag the door prefab here in the Inspector
    private Vector3 doorPosition;  // Position of the door prefab in the scene

    public WaitStaff assignedWaitStaff; // Reference to the assigned WaitStaff

    public void SetMenu(List<Order> availableMenu)
    {
        menu = availableMenu;
    }

    private void Start()
    {
        // If spawn position is not set, default to the top-left corner
        if (spawnPosition == Vector3.zero)
        {
            spawnPosition = new Vector3(-10, 0, 10); // Top-left corner
        }

        // Spawn the customer at the specified spawn position
        transform.position = spawnPosition;

        // Check that the door prefab is assigned and set the door position
        if (doorPrefab == null)
        {
            Debug.LogError("Door prefab is not assigned!");
            return;
        }
        doorPosition = doorPrefab.transform.position; // Use the prefab's position as the exit target

        if (orderPrefab == null)
        {
            Debug.LogError("orderPrefab is not assigned!");
            return;
        }

        // Instantiate the order prefab above the customer and hide it initially
        orderInstance = Instantiate(orderPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity, transform);
        orderInstance.gameObject.SetActive(false); // Initially hide
    }

    public void FindTable(List<Table> tables)
    {
        // Find the nearest available table
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty);

        if (assignedTable != null)
        {
            assignedTable.Occupy();
            currentState = State.LookingForTable; // Start moving towards the table
        }
        else
        {
            Debug.Log("No available tables for the customer.");
        }
    }

    private void Update()
    {
        UpdateState();
    }

    public void UpdateState()
    {
        switch (currentState)
        {
            case State.LookingForTable:
                MoveToTable();
                break;

            case State.WaitingForOrder:
                ShowOrder(); // Show order logic
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

    private void MoveToTable()
    {
        if (assignedTable == null) return;

        // Move towards the assigned table position
        float step = 3f * Time.deltaTime; // Adjust the speed of movement
        transform.position = Vector3.MoveTowards(transform.position, assignedTable.transform.position, step);

        // If the customer reaches the table
        if (transform.position == assignedTable.transform.position)
        {
            currentState = State.WaitingForOrder;
            ShowOrder(); // Show the order once at the table
        }
    }

    // Show a random order (instead of always the first one)
    public void ShowOrder()
    {
        if (menu.Count == 0)
        {
            Debug.LogError("Menu is empty!");
            return;
        }

        // Randomly pick an order from the menu
        currentOrder = menu[Random.Range(0, menu.Count)];

        // Check if the current order's DishPrefab is assigned
        if (currentOrder.DishPrefab == null)
        {
            Debug.LogError("DishPrefab is not assigned for the current order!");
            return;
        }

        // Instantiate the prefab associated with the order
        GameObject orderObject = Instantiate(currentOrder.DishPrefab, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        orderObject.transform.SetParent(transform); // Attach the prefab to the customer for proper positioning
    }

    // Call the assigned waitstaff to take the order
    public Order GiveOrderToWaitStaff()
    {
        // Ensure the customer has an order ready to give
        if (currentOrder != null)
        {
            return currentOrder;  // Return the current order to the WaitStaff
        }
        else
        {
            Debug.LogWarning("Customer has no order to give.");
            return null;
        }
    }

    private void WaitForOrder()
    {
        // Waiting logic (e.g., animations, countdown, etc.) can go here
        // Once food arrives, switch to Eating
        StartEating();
    }

    public bool IsReadyToOrder()
    {
        return currentState == State.WaitingForOrder;
    }

    public void SetStateWaitingForFood()
    {
        currentState = State.WaitingForFood;
    }

    public void SetAssignedWaitStaff(WaitStaff waitStaff)
    {
        assignedWaitStaff = waitStaff;
    }

    private void StartEating()
    {
        eatingTimer = eatingDuration;
        currentState = State.Eating;
    }

    private void Eat()
    {
        eatingTimer -= Time.deltaTime;

        if (eatingTimer <= 0f)
        {
            currentState = State.Leaving;
            if (orderInstance != null) orderInstance.gameObject.SetActive(false); // Hide order image
        }
    }

    private void MoveToDoor()
    {
        // Move towards the door's position (from door prefab)
        float step = 3f * Time.deltaTime; // Adjust the speed of movement to the door
        transform.position = Vector3.MoveTowards(transform.position, doorPosition, step);

        // If the customer reaches the door position, remove them
        if (transform.position == doorPosition)
        {
            if (assignedTable != null)
            {
                assignedTable.Vacate(); // Free up the table
            }
            Destroy(gameObject); // Remove customer from scene
        }
    }
}
