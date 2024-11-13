using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    private enum State { Idle, Cleaning, Serving, TakingOrder, MovingToChef }
    private State currentState = State.Idle;

    private Table targetTable;
    private Customer targetCustomer;

    private Chef chef;  // Reference to the Chef

    public float moveSpeed = 3f; // Movement speed of the waitstaff

    void Start()
    {
        chef = FindObjectOfType<Chef>();  // Find the first Chef in the scene
    }

    void Update()
    {
        UpdateState();  // Update the state of the WaitStaff
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                FindNextTask();
                break;
            case State.Cleaning:
                MoveToTable();
                break;
            case State.Serving:
                ServeOrder();
                break;
            case State.TakingOrder:
                MoveToCustomer();
                break;
            case State.MovingToChef:
                MoveToChef();
                break;
        }
    }

    void FindNextTask()
    {
        // Check if there are customers ready to place an order
        Customer customerNeedingService = FindCustomerNeedingOrder();

        if (customerNeedingService != null)
        {
            targetCustomer = customerNeedingService;
            currentState = State.TakingOrder;
        }
        else
        {
            // Check for other tasks such as cleaning or serving orders if no customers need service
            Table dirtyTable = FindDirtyTable();
            if (dirtyTable != null)
            {
                targetTable = dirtyTable;
                currentState = State.Cleaning;
            }
        }
    }

    Customer FindCustomerNeedingOrder()
    {
        // Find customers who are ready to order
        Customer[] customers = FindObjectsOfType<Customer>();
        foreach (var customer in customers)
        {
            if (customer.IsReadyToOrder())  // Checks if the customer is in the WaitingForOrder state
            {
                return customer;
            }
        }
        return null;
    }

    Table FindDirtyTable()
    {
        // Find dirty tables that need cleaning
        Table[] tables = FindObjectsOfType<Table>();
        foreach (var table in tables)
        {
            if (table.IsDirty)
            {
                return table;
            }
        }
        return null;
    }

    void MoveToCustomer()
    {
        if (targetCustomer == null)
        {
            currentState = State.Idle;
            return;
        }

        // Move toward the customer's position
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetCustomer.transform.position, step);

        // Check if the waitstaff has reached the customer
        if (transform.position == targetCustomer.transform.position)
        {
            TakeOrderFromCustomer();
        }
    }

    void TakeOrderFromCustomer()
    {
        // Ensure the customer has a valid order
        if (targetCustomer != null && targetCustomer.IsReadyToOrder())
        {
            // Call the method that returns the order from the customer
            Order order = targetCustomer.GiveOrderToWaitStaff();  // Get the order from the customer

            if (order != null)
            {
                Debug.Log($"WaitStaff received the order for {order.DishPrefab.name} from customer {targetCustomer.gameObject.name}");

                // Move to the Chef to give the order
                currentState = State.MovingToChef;
            }
            else
            {
                Debug.LogWarning("Failed to retrieve order from customer.");
            }
        }

        // Reset the state after taking the order
        targetCustomer = null;
    }

    void MoveToChef()
    {
        if (chef == null)
        {
            currentState = State.Idle;
            return;
        }

        // Move towards the Chef's position
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, chef.transform.position, step);

        // Check if the waitstaff has reached the Chef
        if (Vector3.Distance(transform.position, chef.transform.position) < 0.1f)
        {
            GiveOrderToChef();
        }
    }

    void GiveOrderToChef()
    {
        // Ensure you have a valid order
        if (targetCustomer != null && targetCustomer.IsReadyToOrder())
        {
            Order order = targetCustomer.GiveOrderToWaitStaff();
            if (order != null)
            {
                // Pass the order to the chef
                chef.ReceiveOrder(order);
                Debug.Log("WaitStaff gave the order to Chef.");
            }
            else
            {
                Debug.LogWarning("Failed to retrieve order from customer.");
            }

            // After giving the order, reset and return to idle state
            targetCustomer = null;
            currentState = State.Idle;
        }
    }


    void MoveToTable()
    {
        if (targetTable == null)
        {
            currentState = State.Idle;
            return;
        }

        // Move towards the table's position
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetTable.transform.position, step);

        // Check if the waitstaff has reached the table
        if (transform.position == targetTable.transform.position)
        {
            CleanTable();
        }
    }

    void CleanTable()
    {
        // Clean the table
        targetTable.Clean();

        currentState = State.Idle;
        targetTable = null; // Reset after cleaning the table
    }

    void ServeOrder()
    {
        // Code to serve food to a customer (implementation pending)
        currentState = State.Idle;
    }
}
