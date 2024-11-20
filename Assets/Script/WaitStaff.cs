using System.Collections.Generic;
using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    public enum State { Idle, GotoTable, TakingOrderFromCus, GotoChef, TakingDoneOrderFromChef, DeliveringOrderToCustomer }
    public State currentState = State.Idle;

    private Queue<Customer> customerQueue = new Queue<Customer>(); // Queue of customers to serve
    private Customer currentCustomer; // The customer currently being served
    private Order currentOrder; // The order being handled by the waitstaff

    public float moveSpeed = 3f; // Movement speed of the waitstaff
    public Transform chefLocation; // Position of the chef

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
                Debug.Log("WaitStaff is taking the order from the customer.");
                TakeOrderFromCustomer();
                break;

            case State.GotoChef:
                Debug.Log("WaitStaff is moving to the chef.");
                MoveToChef();
                break;

            case State.TakingDoneOrderFromChef:
                //RetrieveOrderFromChef();
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
            currentCustomer.NotifyWaitStaffArrived(); // Notify the customer
            currentState = State.TakingOrderFromCus;
        }
    }


    private void TakeOrderFromCustomer()
    {
        if (currentCustomer == null || !currentCustomer.IsReadyToOrder())
        {
            Debug.LogWarning($"WaitStaff: Customer {currentCustomer?.gameObject.name ?? "None"} is not ready or null.");
            currentState = State.Idle;
            return;
        }

        currentOrder = currentCustomer.GiveOrderToWaitStaff();
        if (currentOrder != null)
        {
            Debug.Log($"WaitStaff received order: {currentOrder.DishName} from Customer {currentCustomer.gameObject.name}");

            // Assign the correct prefab based on the dish name
            switch (currentOrder.DishName.ToLower())
            {
                case "salad":
                    currentOrder.DishPrefab = saladPrefab;
                    break;
                case "burger":
                    currentOrder.DishPrefab = burgerPrefab;
                    break;
                case "pizza":
                    currentOrder.DishPrefab = pizzaPrefab;
                    break;
                default:
                    Debug.LogWarning($"No prefab assigned for dish: {currentOrder.DishName}");
                    break;
            }

            currentState = State.GotoChef;
        }
        else
        {
            Debug.LogWarning($"WaitStaff: Failed to receive order from Customer {currentCustomer.gameObject.name}");
            currentState = State.Idle;
        }
    }



    private void MoveToChef()
    {
        if (chefLocation == null)
        {
            Debug.LogError("Chef location is not set.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, chefLocation.position, step);

        if (Vector3.Distance(transform.position, chefLocation.position) < 0.5f)
        {
            Debug.Log("WaitStaff reached the chef.");
            DeliverOrderToChef();
        }
    }
    private void DeliverOrderToChef()
    {
        if (chefLocation == null)
        {
            Debug.LogError("Chef location is not set. Cannot deliver order.");
            currentState = State.Idle;
            return;
        }

        if (currentOrder == null)
        {
            Debug.LogWarning("No order to deliver to the chef. Returning to Idle state.");
            currentState = State.Idle;
            return;
        }

        Chef chef = chefLocation.GetComponent<Chef>();
        if (chef != null)
        {
            chef.ReceiveOrder(currentOrder);
            Debug.Log($"Order '{currentOrder.DishName}' delivered to the chef.");
        }

        currentOrder = null; // Clear the current order
        currentState = State.Idle; // WaitStaff ready to serve the next customer
    }

    /*private void RetrieveOrderFromChef()
    {
        if (chefLocation == null || currentCustomer == null)
        {
            Debug.LogWarning("Chef or customer information missing.");
            currentState = State.Idle;
            return;
        }

        Chef chef = chefLocation.GetComponent<Chef>();
        if (chef != null && chef.HasOrderForCustomer(currentCustomer))
        {
            currentOrder = chef.GivePreparedOrderToWaitStaff();
            CreateHeldDish(); // Create the visual representation of the dish
            currentState = State.DeliveringOrderToCustomer;
            Debug.Log($"Order '{currentOrder.DishName}' retrieved from chef. Ready to deliver to customer.");
        }
    }*/


    private void DeliverOrderToCustomer()
    {
        if (currentCustomer == null)
        {
            Debug.LogError("No customer to deliver the order to.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentCustomer.transform.position, step);

        if (Vector3.Distance(transform.position, currentCustomer.transform.position) < 0.5f)
        {
            Debug.Log($"Order delivered to Customer {currentCustomer.gameObject.name}.");
            ResetWaitStaff();
        }
    }

    private void ResetWaitStaff()
    {
        currentCustomer = null;
        currentOrder = null;
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
        heldDishInstance.transform.parent = transform;
    }

    public bool IsIdle()
    {
        //Debug.LogError("Assigned Staff: " + currentState.ToString());
        return currentState == State.Idle;
    }
}