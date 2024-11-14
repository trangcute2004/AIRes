using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    private enum State { Idle, TakingOrder, DeliveringOrder }
    private State currentState = State.Idle;

    private Customer targetCustomer;
    private Order currentOrder;
    public float moveSpeed = 3f;
    public Transform chefLocation;

    private GameObject heldDishInstance; // To visually represent the dish being carried


    private void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                break;
            case State.TakingOrder:
                MoveToCustomer();
                break;
            case State.DeliveringOrder:
                MoveToChef();
                break;
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
        Debug.Log($"WaitStaff {gameObject.name} assigned to Customer {customer.gameObject.name}");
        currentState = State.TakingOrder;
    }


    private void MoveToCustomer()
    {
        if (targetCustomer == null)
        {
            currentState = State.Idle;
            return;
        }

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetCustomer.transform.position, step);

        if (Vector3.Distance(transform.position, targetCustomer.transform.position) < 0.1f)
        {
            TakeOrderFromCustomer();
        }
    }

    public void TakeOrderFromCustomer()
    {
        if (targetCustomer == null)
        {
            Debug.LogWarning("No customer is assigned to this WaitStaff.");
            currentState = State.Idle;
            return;
        }

        if (!targetCustomer.IsReadyToOrder())
        {
            Debug.LogWarning($"Customer {targetCustomer.gameObject.name} is not ready to order.");
            currentState = State.Idle;
            return;
        }

        // Take the order from the customer
        currentOrder = targetCustomer.GiveOrderToWaitStaff();
        if (currentOrder != null)
        {
            Debug.Log($"Order received: {currentOrder.DishName}");
            CreateHeldDish(currentOrder.DishPrefab); // Show the dish being carried
            currentState = State.DeliveringOrder;
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

        if (Vector3.Distance(transform.position, chefLocation.position) < 0.1f)
        {
            DeliverOrderToChef();
        }
    }

    private void DeliverOrderToChef()
    {
        if (chefLocation == null || currentOrder == null)
        {
            Debug.LogError("Cannot deliver order: Missing chef location or order.");
            currentState = State.Idle;
            return;
        }

        Chef chef = chefLocation.GetComponent<Chef>();
        if (chef != null)
        {
            chef.ReceiveOrder(currentOrder);
            Debug.Log($"Order {currentOrder.DishName} delivered to chef.");
        }

        Destroy(heldDishInstance); // Clean up the dish visual representation
        currentOrder = null;
        currentState = State.Idle;
    }

    private void CreateHeldDish(GameObject dishPrefab)
    {
        if (dishPrefab == null)
        {
            Debug.LogError("Dish prefab is missing!");
            return;
        }

        if (heldDishInstance != null)
        {
            Destroy(heldDishInstance); // Clean up any existing dish representation
        }

        heldDishInstance = Instantiate(dishPrefab, transform.position + Vector3.up, Quaternion.identity);
        heldDishInstance.transform.parent = this.transform; // Attach to waitstaff
    }

    public bool IsIdle()
    {
        return currentState == State.Idle;
    }
}
