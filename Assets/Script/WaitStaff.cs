using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    // Define the states for the waitstaff
    private enum State { Idle, GotoTable, TakingOrderFromCus, GotoChef, TakingDoneOrderFromChef, DeliveringOrderToCustomer }
    private State currentState = State.Idle;

    private Customer targetCustomer; // The customer currently being served
    private Order currentOrder; // The order being handled by the waitstaff

    public float moveSpeed = 3f; // Movement speed of the waitstaff
    public Transform chefLocation; // Position of the chef

    private GameObject heldDishInstance; // Visual representation of the dish being carried

    private void Update()
    {
        UpdateState(); // Handle state-specific behavior
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                // Waitstaff is idle, waiting for an assignment
                break;
            case State.GotoTable:
                break;
            case State.TakingOrderFromCus:
                MoveToCustomer();
                break;
            case State.GotoChef:
                break;
            case State.TakingDoneOrderFromChef:
                break;
            case State.DeliveringOrderToCustomer:
                MoveToChef();
                break;
        }
    }

    // Assign a customer to the waitstaff and start taking the order
    public void SetTargetCustomer(Customer customer)
    {
        if (customer == null)
        {
            Debug.LogError("SetTargetCustomer called with null customer.");
            return;
        }

        targetCustomer = customer;
        Debug.Log($"WaitStaff {gameObject.name} assigned to Customer {customer.gameObject.name}");
        currentState = State.TakingDoneOrderFromChef;
    }

    // Move the waitstaff towards the customer's position
    private void MoveToCustomer()
    {
        if (targetCustomer == null)
        {
            Debug.LogWarning("No target customer. Returning to Idle state.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetCustomer.transform.position, step);

        // Check if the waitstaff has reached the customer
        if (Vector3.Distance(transform.position, targetCustomer.transform.position) < 0.5f) // Adjust proximity threshold if needed
        {
            TakeOrderFromCustomer();
        }
    }

    // Take the order from the customer
    private void TakeOrderFromCustomer()
    {
        if (targetCustomer == null)
        {
            Debug.LogWarning("No target customer to take order from.");
            currentState = State.Idle;
            return;
        }

        if (!targetCustomer.IsReadyToOrder())
        {
            Debug.LogWarning($"Customer {targetCustomer.gameObject.name} is not ready to order.");
            currentState = State.Idle;
            return;
        }

        // Get the order from the customer
        currentOrder = targetCustomer.GiveOrderToWaitStaff();
        if (currentOrder != null)
        {
            Debug.Log($"Order received: {currentOrder.DishName}");
            CreateHeldDish(currentOrder.DishPrefab); // Display the dish being carried
            currentState = State.DeliveringOrderToCustomer;
        }
    }

    // Move the waitstaff towards the chef's location
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

        // Check if the waitstaff has reached the chef
        if (Vector3.Distance(transform.position, chefLocation.position) < 0.5f) // Adjust proximity threshold if needed
        {
            DeliverOrderToChef();
        }
    }

    // Deliver the order to the chef
    private void DeliverOrderToChef()
    {
        if (chefLocation == null || currentOrder == null)
        {
            Debug.LogError("Cannot deliver order: Missing chef location or order.");
            currentState = State.Idle;
            return;
        }

        // Pass the order to the chef
        Chef chef = chefLocation.GetComponent<Chef>();
        if (chef != null)
        {
            chef.ReceiveOrder(currentOrder);
            Debug.Log($"Order {currentOrder.DishName} delivered to chef.");
        }

        // Clean up the held dish visual
        Destroy(heldDishInstance);
        heldDishInstance = null;

        // Reset for next task
        currentOrder = null;
        targetCustomer = null;
        currentState = State.Idle;
    }

    // Create a visual representation of the dish being carried
    private void CreateHeldDish(GameObject dishPrefab)
    {
        if (dishPrefab == null)
        {
            Debug.LogError("Dish prefab is missing!");
            return;
        }

        if (heldDishInstance != null)
        {
            Destroy(heldDishInstance); // Remove the existing visual if any
        }

        heldDishInstance = Instantiate(dishPrefab, transform.position + Vector3.up, Quaternion.identity);
        heldDishInstance.transform.parent = this.transform; // Attach the dish to the waitstaff
    }

    // Check if the waitstaff is idle and ready for a new task
    public bool IsIdle()
    {
        if (targetCustomer !=  null)
        { return currentState == State.TakingDoneOrderFromChef; }
        return currentState == State.Idle;
    }
}
