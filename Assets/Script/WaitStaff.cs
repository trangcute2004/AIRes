using System.Collections.Generic;
using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    public enum State { Idle, GotoTable, TakingOrderFromCus, GotoDishStack, TakeOrđerFromDishStack, DeliveringOrderToCustomer }
    public State currentState = State.Idle;

    private Queue<Customer> customerQueue = new Queue<Customer>(); // Queue of customers to serve
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
                if (customerQueue.Count > 0)
                {
                    AssignNextCustomer();
                }
                break;

            case State.GotoTable:
                Debug.Log("WaitStaff is moving to the table.");
                MoveToCustomer();
                break;

            case State.TakingOrderFromCus:
  
                TakeOrderFromCustomer();
                break;

            case State.GotoDishStack:
                Debug.Log("WaitStaff is moving to the dish stack.");
                MoveToDishStack();
                break;

            case State.TakeOrđerFromDishStack:
                
                break;

            case State.DeliveringOrderToCustomer:
                Debug.Log("WaitStaff is delivering the order to the customer.");
                DeliverOrderToCustomer();
                break;
        }
    }

    public void AddCustomerToQueue(Customer customer)
    {
        if (customer != null)
        {
            customerQueue.Enqueue(customer);
            Debug.Log($"Customer {customer.gameObject.name} added to the queue.");
        }
    }

    private void AssignNextCustomer()
    {
        if (customerQueue.Count == 0)
        {
            Debug.LogWarning("No customers in queue.");
            currentState = State.Idle;
            return;
        }

        currentCustomer = customerQueue.Dequeue();
        Debug.Log($"Serving Customer {currentCustomer.gameObject.name}.");
        currentState = State.GotoTable;
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

        // Check if the customer is in the right state to give the order
        Debug.Log($"WaitStaff checking if customer is ready. Customer state: {currentCustomer.currentState}");
        if (currentCustomer.currentState != Customer.State.GiveOrderToStaff || currentCustomer.HasGivenOrder)
        {
            Debug.LogWarning($"WaitStaff: Customer {currentCustomer.gameObject.name} is not ready to order.");
            currentState = State.Idle;
            return;
        }

        // Proceed with taking the order
        currentOrder = currentCustomer.GiveOrderToWaitStaff();
        if (currentOrder != null)
        {
            Debug.Log($"WaitStaff received order: {currentOrder.DishName} from Customer {currentCustomer.gameObject.name}");
            currentState = State.GotoDishStack; // Transition to the next state
        }
        else
        {
            Debug.LogWarning($"WaitStaff: Failed to receive order from Customer {currentCustomer.gameObject.name}");
            currentState = State.Idle;
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
            Destroy(heldDishInstance); // Destroy the dish when delivered

            // Mark the order as delivered
            if (currentOrder != null)
            {
                currentOrder.IsDelivered = true;
            }

            ResetWaitStaff();  // Reset for the next customer
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
        //Debug.LogError("Assigned Staff: " + currentState.ToString());
        return currentState == State.Idle;
    }
}