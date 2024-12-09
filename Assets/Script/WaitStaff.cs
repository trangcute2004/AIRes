using System.Collections.Generic;
using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    public enum State { Idle, GotoTable, TakingOrderFromCus, GotoDishStack, TakeOrderFromDishStack, DeliveringOrderToCustomer }
    public State currentState = State.Idle;
    public Vector3 defaultPosition;


    private Queue<Customer> customerQueueForService = new Queue<Customer>(); // Queue of customers waiting to be served
    private Queue<Table> cleaningQueue = new Queue<Table>();  // Queue for tables to clean

    private Customer currentCustomer; // The customer currently being served
    private Order currentOrder; // The order being handled by the waitstaff

    public float moveSpeed = 3f; // Movement speed of the waitstaff
    public Transform chefLocation; // Position of the chef
    public Transform dishStackLocation;  // Dish stack position

    public GameObject saladPrefab;  // Assign in the Inspector
    public GameObject burgerPrefab; // Assign in the Inspector
    public GameObject pizzaPrefab;  // Assign in the Inspector
    private GameObject heldDishInstance;

    private void Start()
    {
        currentState = State.Idle;
        defaultPosition = transform.position;
    }

    private void Update()
    {
        UpdateState();

    }

    private void UpdateState()
    {
        Debug.Log("STAFF STATE: " + currentState.ToString());
        switch (currentState)
        {
            case State.Idle:
                if (customerQueueForService.Count > 0)
                {
                    AssignNextCustomer(); // Assign next customer to serve
                }

                if (cleaningQueue.Count > 0)
                {
                    Table tableToClean = cleaningQueue.Dequeue();  // Get the next dirty table
                    CleanTable(tableToClean);
                }

                // Move to the default position if Idle
                transform.position = Vector3.MoveTowards(transform.position, defaultPosition, moveSpeed * Time.deltaTime);
                break;


            case State.GotoTable:
                MoveToCustomer();
                break;

            case State.TakingOrderFromCus:
                TakeOrderFromCustomer();
                break;

            case State.GotoDishStack:
                MoveToDishStack();
                break;

            case State.DeliveringOrderToCustomer:
                DeliverOrderToCustomer();
                break;
        }
    }

    // Add a customer to the queue for service, prioritizing those who are waiting at the table (State.WaitingStaffToCome)
    public void AddCustomerToQueue(Customer customer)
    {
        if (customer != null)
        {
            if (customer.currentState == Customer.State.WaitingStaffToCome)
            {
                // Add the waiting customer to the front of the queue (prioritize them)
                customerQueueForService.Enqueue(customer);
                Debug.Log($"Customer {customer.gameObject.name} added to the service queue (priority).");
            }
            else
            {
                // Add customers who are not waiting at the table to the back of the queue
                customerQueueForService.Enqueue(customer);
                Debug.Log($"Customer {customer.gameObject.name} added to the service queue.");
            }

            // If the staff is idle, immediately start serving the next customer
            if (currentState == State.Idle)
            {
                AssignNextCustomer();  // Start serving the first customer in the queue
            }
        }
    }

    public void AddCleaningTask(Table table)
    {
        if (!cleaningQueue.Contains(table) && table.IsDirty)
        {
            cleaningQueue.Enqueue(table);  // Add dirty table to the cleaning queue
        }
    }

    private void CleanTable(Table table)
    {
        if (table != null)
        {
            table.Clean();  // Clean the table
            Debug.Log($"WaitStaff cleaned table {table.TableNumber}.");  // Optional: Debug message
        }
    }

    // Assign the next customer to serve (prioritize those who are waiting at the table)
    private void AssignNextCustomer()
    {
        if (customerQueueForService.Count == 0)
        {
            Debug.LogWarning("No customers in service queue.");
            currentState = State.Idle;
            return;
        }

        // First, check if there are any customers who are waiting at their table (WaitingStaffToCome)
        Customer waitingCustomer = null;
        foreach (Customer customer in customerQueueForService)
        {
            if (customer.currentState == Customer.State.WaitingStaffToCome)
            {
                waitingCustomer = customer;
                break; // Stop as soon as we find a waiting customer
            }
        }

        // If there is a waiting customer, serve them first, otherwise serve the first customer in the queue
        currentCustomer = waitingCustomer ?? customerQueueForService.Dequeue();
        Debug.Log($"Serving Customer {currentCustomer.gameObject.name}.");
        currentState = State.GotoTable; // Move to the next customer
    }

    private void MoveToCustomer()
    {
        if (currentCustomer == null)
        {
            Debug.LogWarning("No target customer. Returning to Idle state.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentCustomer.transform.position, step);

        if (Vector3.Distance(transform.position, currentCustomer.transform.position) < 0.5f)
        {
            Debug.Log($"WaitStaff: Reached Customer {currentCustomer.gameObject.name}'s table.");
            currentCustomer.NotifyWaitStaffArrived(); // Notify the customer to show the dish prefab
            currentState = State.TakingOrderFromCus;  // Switch to order-taking state
        }
    }

    private void TakeOrderFromCustomer()
    {
        if (currentCustomer == null)
        {
            Debug.LogWarning("WaitStaff: No customer assigned.");
            currentState = State.Idle;
            return;
        }

        // If the customer is ready to give the order, proceed with taking the order
        if (currentCustomer.currentState == Customer.State.GiveOrderToWaitStaff || currentCustomer.currentState == Customer.State.OrderGiven)
        {
            currentOrder = currentCustomer.GiveOrderToWaitStaff();
            if (currentOrder != null)
            {
                Debug.Log($"WaitStaff received order: {currentOrder.DishName} from Customer {currentCustomer.gameObject.name}");
                currentCustomer.OnOrderTakenByWaitStaff(); // Transition customer to OrderGiven state
                currentState = State.GotoDishStack; // Transition to moving to dish stack
            }
            else
            {
                Debug.LogWarning($"WaitStaff: Failed to receive order from Customer {currentCustomer.gameObject.name}");
                currentState = State.Idle;
            }
        }
        else
        {
            currentState = State.Idle;  // Return to idle if not in the correct state
        }
    }

    private void MoveToDishStack()
    {
        if (dishStackLocation == null)
        {
            Debug.LogError("Dish Stack location not set.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, dishStackLocation.position, step);

        if (Vector3.Distance(transform.position, dishStackLocation.position) < 0.5f)
        {
            Debug.Log("WaitStaff reached the dish stack.");
            CreateHeldDish(); // Create the dish from the order's prefab
            currentState = State.DeliveringOrderToCustomer;  // Move to delivering to the customer
        }
    }

    private void DeliverOrderToCustomer()
    {
        if (currentCustomer == null || heldDishInstance == null)
        {
            Debug.LogError("No customer or dish to deliver.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentCustomer.transform.position, step);

        if (Vector3.Distance(transform.position, currentCustomer.transform.position) < 0.5f)
        {
            Debug.Log($"Order delivered to Customer {currentCustomer.gameObject.name}.");
            Destroy(heldDishInstance);  // Destroy the dish when delivered

            if (currentOrder != null)
            {
                currentOrder.IsDelivered = true;  // Mark the order as delivered
                currentCustomer.OnOrderDelivered();  // Notify the customer that the order is delivered
            }

            ResetWaitStaff();  // Reset for the next customer

            // Automatically serve the next customer in the queue
            AssignNextCustomer();  // Call to serve the next customer
        }
    }

    private void ResetWaitStaff()
    {
        currentCustomer = null;
        currentOrder = null;
        heldDishInstance = null;  // Remove held dish reference
        currentState = State.Idle;
        Debug.Log("WaitStaff ready for the next customer.");
    }

    private void CreateHeldDish()
    {
        if (currentOrder == null || currentOrder.DishPrefab == null) return;

        if (heldDishInstance != null)
        {
            Destroy(heldDishInstance);
        }

        heldDishInstance = Instantiate(currentOrder.DishPrefab, transform.position + Vector3.up, Quaternion.identity);
        heldDishInstance.transform.parent = transform;  // Parent to waitstaff, so it follows the movement
        Debug.Log($"Dish '{currentOrder.DishName}' is now held by WaitStaff.");
    }

    public bool IsIdle()
    {
        return currentState == State.Idle;
    }
}
