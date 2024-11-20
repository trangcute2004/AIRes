using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    public enum State { Idle, GotoTable, TakingOrderFromCus, GotoChef, TakingDoneOrderFromChef, DeliveringOrderToCustomer }
    public State currentState = State.Idle;

    private Customer targetCustomer; // The customer currently being served
    private Order currentOrder; // The order being handled by the waitstaff

    public float moveSpeed = 3f; // Movement speed of the waitstaff
    public Transform chefLocation; // Position of the chef

    private GameObject heldDishInstance; // Visual representation of the dish being carried

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
                //currentState = State.GotoTable;
                break;

            case State.GotoTable:
                Debug.Log("WaitStaff is moving to the table.");
                MoveToCustomer();
                break;

                //case State.TakingOrderFromCus:
                //    Debug.Log("WaitStaff is taking the order from the customer.");
                //    TakeOrderFromCustomer();
                //    break;

                //case State.GotoChef:
                //    Debug.Log("WaitStaff is moving to the chef.");
                //    MoveToChef();
                //    break;

                //case State.TakingDoneOrderFromChef:
                //    // Add logic if needed
                //    break;

                //case State.DeliveringOrderToCustomer:
                //    Debug.Log("WaitStaff is delivering the order to the customer.");
                //    DeliverOrderToCustomer();
                //    break;
        }
    }

    public void SetTargetCustomer(Customer customer)
    {
        if (customer == null)
        {
            Debug.LogError("SetTargetCustomer called with null customer.");
            return;
        }

        targetCustomer = customer;
        Debug.LogError("1231232");
        currentState = State.GotoTable;
        Debug.Log($"WaitStaff assigned to Customer {customer.gameObject.name}. Moving to table.");
    }

    private void MoveToCustomer()
    {
        Debug.LogError("213333333333333333");
        if (targetCustomer == null)
        {
            Debug.LogWarning("No target customer. Returning to Idle state.");
            currentState = State.Idle;
            return;
        }
        Debug.Log("GoHere");
        // Move towards the customer
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetCustomer.transform.position, step);

        float distance = Vector3.Distance(transform.position, targetCustomer.transform.position);
        Debug.Log($"WaitStaff moving to Customer. Distance remaining: {distance}");

        // Check if the waitstaff has reached the customer
        if (distance < 0.5f) // Adjust threshold if needed
        {
            Debug.Log($"WaitStaff reached Customer {targetCustomer.gameObject.name}'s table.");
            currentState = State.TakingOrderFromCus;
        }
    }

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

        currentOrder = targetCustomer.GiveOrderToWaitStaff();
        if (currentOrder != null)
        {
            Debug.Log($"Order received: {currentOrder.DishName}");
            CreateHeldDish(currentOrder.DishPrefab); // Display the dish being carried
            currentState = State.GotoChef;
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

        float distance = Vector3.Distance(transform.position, chefLocation.position);
        Debug.Log($"WaitStaff moving to Chef. Distance remaining: {distance}");

        if (distance < 0.5f)
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

        // Simulate order delivery to the chef
        Chef chef = chefLocation.GetComponent<Chef>();
        if (chef != null)
        {
            chef.ReceiveOrder(currentOrder); // Assuming Chef has a ReceiveOrder method
            Debug.Log($"Order '{currentOrder.DishName}' successfully delivered to the chef.");
        }
        else
        {
            Debug.LogError("Chef object does not have a Chef component. Cannot deliver order.");
        }

        // Reset current order and held dish after delivery
        Destroy(heldDishInstance);
        heldDishInstance = null;
        currentOrder = null;

        // Transition to the next state or reset to Idle
        currentState = State.DeliveringOrderToCustomer; // Ready to take the prepared dish back to the customer
        Debug.Log("WaitStaff transitioning to DeliveringOrderToCustomer state.");
    }

    private void DeliverOrderToCustomer()
    {
        if (targetCustomer == null)
        {
            Debug.LogError("No customer to deliver the order to.");
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetCustomer.transform.position, step);

        float distance = Vector3.Distance(transform.position, targetCustomer.transform.position);
        Debug.Log($"WaitStaff delivering to Customer. Distance remaining: {distance}");

        if (distance < 0.5f)
        {
            Debug.Log($"Order delivered to Customer {targetCustomer.gameObject.name}.");
            ResetWaitStaff();
        }
    }

    private void ResetWaitStaff()
    {
        targetCustomer = null;
        currentOrder = null;
        Destroy(heldDishInstance);
        heldDishInstance = null;
        currentState = State.Idle;
        Debug.Log("WaitStaff reset to Idle state.");
    }

    private void CreateHeldDish(GameObject dishPrefab)
    {
        if (dishPrefab == null) return;

        if (heldDishInstance != null)
        {
            Destroy(heldDishInstance);
        }

        heldDishInstance = Instantiate(dishPrefab, transform.position + Vector3.up, Quaternion.identity);
        heldDishInstance.transform.parent = transform;
    }

    public bool IsIdle()
    {
        Debug.LogError("Assigned Staff: " + currentState.ToString());
        return currentState == State.Idle;
    }
}