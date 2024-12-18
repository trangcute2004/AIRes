using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    public enum State { Idle, GotoTable, TakingOrderFromCus, GotoDishStack, TakeOrderFromDishStack, DeliveringOrderToCustomer }
    public State currentState = State.Idle;
    public Vector3 defaultPosition;


    public Queue<Customer> customerQueueForService = new Queue<Customer>(); // Queue of customers waiting to be served
    private Queue<Table> cleaningQueue = new Queue<Table>();  // Queue for tables to clean

    private Customer currentCustomer; // The customer currently being served
    private Order currentOrder; // The order being handled by the waitstaff

    public float moveSpeed = 3f; // Movement speed of the waitstaff
    public Transform chefLocation; // Position of the chef
    public Transform dishStackLocation;  // Dish stack position

    public GameObject saladPrefab;  // Assign in the Inspector
    public GameObject burgerPrefab; // Assign in the Inspector
    public GameObject pizzaPrefab;  // Assign in the Inspector
    //// It will be used to keep track of the dish the customer is interacting with
    private GameObject heldDishInstance;

    private void Start()
    {
        // Initialize the wait staff's state to 'Idle' when the game starts.
        currentState = State.Idle;
        // Store the wait staff's initial position when the game starts
        defaultPosition = transform.position;
    }

    private void Update()
    {
        UpdateState();

    }

    private void UpdateState()
    {
        //track the current state of the waitstaff in the console
        Debug.Log("STAFF STATE: " + currentState.ToString());
        switch (currentState)
        {
            case State.Idle:
                // Check if there are customers in the queue waiting to be served.
                if (customerQueueForService.Count > 0)
                {
                    AssignNextCustomer(); // Assign next customer to serve
                }

                // Check if there are dirty tables in the cleaning queue.
                if (cleaningQueue.Count > 0)
                {
                    //Dequeue the next dirty table from the cleaning queue.
                    Table tableToClean = cleaningQueue.Dequeue();  
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
        // Check if the customer is not null
        if (customer != null)
        {
            // Check if the customer is in the 'WaitingStaffToCome' state
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

    // Assign the next customer to serve (prioritize those who are waiting at the table)
    private void AssignNextCustomer()
    {
        // Check if the service queue is empty
        if (customerQueueForService.Count == 0)
        {
            Debug.LogWarning("No customers in service queue.");
            currentState = State.Idle; // Set the waitstaff state to idle when there's no one to serve.
            return;
        }

        // check if there are any customers who are waiting at their table (WaitingStaffToCome)
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
        Debug.Log($"Serving Customer {currentCustomer.gameObject.name}. Customer is at table {currentCustomer.assignedTable.name}");
        currentState = State.GotoTable; // Move to the next customer
    }

    // Moves the waitstaff to the customer's position
    private void MoveToCustomer()
    {
        // Check if there is a current customer assigned
        if (currentCustomer == null)
        {
            Debug.LogWarning("No target customer. Returning to Idle state.");
            currentState = State.Idle; // Set the waitstaff state to Idle since there's no customer to serve.
            return;
        }

        //the step size for the wait staff's movement
        float step = moveSpeed * Time.deltaTime;
        // Move the waitstaff to the customer's position.
        transform.position = Vector3.MoveTowards(transform.position, currentCustomer.transform.position, step);

        // Check if the waitstaff is close enough to the customer
        if (Vector3.Distance(transform.position, currentCustomer.transform.position) < 0.5f)
        {
            Debug.Log($"WaitStaff: Reached Customer {currentCustomer.gameObject.name}'s table.");
            currentCustomer.NotifyWaitStaffArrived(); // Notify the customer to show the dish prefab
            currentState = State.TakingOrderFromCus;  // Switch to order-taking state
        }
    }

    // taking the customer's order
    private void TakeOrderFromCustomer()
    {
        // Check if there is no customer assigned
        if (currentCustomer == null)
        {
            Debug.LogWarning("WaitStaff: No customer assigned.");
            currentState = State.Idle; // Set the waitstaff's state to Idle since there is no customer.
            return;
        }

        // If the customer is ready to give the order, proceed with taking the order
        if (currentCustomer.currentState == Customer.State.GiveOrderToWaitStaff || currentCustomer.currentState == Customer.State.OrderGiven)
        {
            //receive the customer's order
            currentOrder = currentCustomer.GiveOrderToWaitStaff();
            // If the order is valid 
            if (currentOrder != null)
            {
                Debug.Log($"WaitStaff received order: {currentOrder.DishName} from Customer {currentCustomer.gameObject.name}");
                currentCustomer.OnOrderTakenByWaitStaff(); // Transition customer to OrderGiven state
                currentState = State.GotoDishStack; // Transition wait staff to moving to dish stack
            }
            else
            {
                Debug.LogWarning($"WaitStaff: Failed to receive order from Customer {currentCustomer.gameObject.name}");
                currentState = State.Idle; // Return to Idle state if no order was received
            }
        }
        else
        {
            currentState = State.Idle;  // Return to idle if not in the correct state
        }
    }

    //Moves the WaitStaff to the dish stack to collect the dish
    private void MoveToDishStack()
    {
        // Check if the dish stack location has been set
        if (dishStackLocation == null)
        {
            Debug.LogError("Dish Stack location not set.");
            currentState = State.Idle; // Return to idle state if the dish stack location is not assigned.
            return;
        }

        // the step size of wait staff's movement to dish stack
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, dishStackLocation.position, step);

        // Check if the waitstaff has reached the dish stack location by
        if (Vector3.Distance(transform.position, dishStackLocation.position) < 0.5f)
        {
            Debug.Log("WaitStaff reached the dish stack.");
            CreateHeldDish(); // Create the dish from the order's prefab
            currentState = State.DeliveringOrderToCustomer;  // transition to delivering to the customer state
        }
    }

    // Delivers the order to the customer
    private void DeliverOrderToCustomer()
    {
        // Check if there's no customer or no dish to deliver
        if (currentCustomer == null || heldDishInstance == null)
        {
            Debug.LogError("No customer or dish to deliver.");
            currentState = State.Idle;// Return to idle state if no customer or dish exists.
            return;
        }

        // the step size of wait staff's movement to customer position
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentCustomer.transform.position, step);

        // Check if the waitstaff has reached the customer’s position to deliver the order
        if (Vector3.Distance(transform.position, currentCustomer.transform.position) < 0.5f)
        {
            Debug.Log($"Order delivered to Customer {currentCustomer.gameObject.name}.");
            Destroy(heldDishInstance);  // Destroy the dish prefab when delivered

            // If the current order is valid
            if (currentOrder != null)
            {
                currentOrder.IsDelivered = true;  // Mark the order as delivered
                currentCustomer.OnOrderDelivered();  // Notify the customer that the order is delivered
            }

            ResetWaitStaff();  // Reset for the next customer

            // If there are more customers in the queue, assign the next customer to serve.
            if (customerQueueForService.Count > 0)
            {
                AssignNextCustomer();  // serve the next customer in line
            }
            else
            {
                currentState = State.Idle;  // No customers, go idle
            }
        }
    }

    // Resets the waitstaff to prepare for the next customer or task
    private void ResetWaitStaff()
    {
        currentCustomer = null; // Clear the reference to the current customer.
        currentOrder = null; // Clear the reference to the current order.
        heldDishInstance = null;  // Remove held dish reference
        currentState = State.Idle;
        Debug.Log("WaitStaff ready for the next customer.");
    }

    // Creates and holds the dish for the current order
    private void CreateHeldDish()
    {
        // If there is no order or the order's dish prefab is missing
        if (currentOrder == null || currentOrder.DishPrefab == null) return;

        // If the waitstaff is already holding a dish, destroy the previous one before creating a new on
        if (heldDishInstance != null)
        {
            Destroy(heldDishInstance);
        }

        // Instantiate the new dish at the waitstaff's current position
        heldDishInstance = Instantiate(currentOrder.DishPrefab, transform.position + Vector3.up, Quaternion.identity);
        heldDishInstance.transform.parent = transform;  // Parent to waitstaff, so it follows the movement
        Debug.Log($"Dish '{currentOrder.DishName}' is now held by WaitStaff.");
    }

    // Checks if the waitstaff is currently idle
    public bool IsIdle()
    {
        //the waitstaff is Idle
        return currentState == State.Idle;
    }

    // Add a cleaning task for a dirty table
    public void AddCleaningTask(Table table)
    {
        // Check if the table is not already in the cleaning queue and is dirty
        if (!cleaningQueue.Contains(table) && table.IsDirty)
        {
            cleaningQueue.Enqueue(table);  // Add dirty table to the cleaning queue
        }
    }

    // Clean a table.
    private void CleanTable(Table table)
    {
        // Check if the table is not null
        if (table != null)
        {
            table.Clean();  // Clean the table
            Debug.Log($"WaitStaff cleaned table {table.TableNumber}.");
        }
    }
}
