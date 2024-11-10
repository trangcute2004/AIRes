using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingForOrder, Eating, Leaving }
    private State currentState = State.LookingForTable;
    private Table assignedTable;
    private Order currentOrder;

    private float eatingDuration = 5f;
    private float eatingTimer;

    private List<Order> menu = new List<Order>();

    public GameObject orderPrefab; // Drag Image prefab here (if using images)
    private GameObject orderInstance;

    public Vector3 spawnPosition; // Position where the customer spawns (e.g., a corner)

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

        transform.position = spawnPosition; // Spawn the customer at the corner

        if (orderPrefab == null)
        {
            Debug.LogError("orderPrefab is not assigned!");
            return;
        }

        // Instantiate the prefab
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
                WaitForOrder();
                break;

            case State.Eating:
                Eat();
                break;

            case State.Leaving:
                Leave();
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

    private void WaitForOrder()
    {
        if (currentOrder != null && currentState == State.WaitingForOrder)
        {
            StartEating();
        }
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

    private void Leave()
    {
        if (assignedTable != null)
        {
            assignedTable.Vacate(); // Free up the table
        }
        Destroy(gameObject); // Remove customer from scene
    }
}
