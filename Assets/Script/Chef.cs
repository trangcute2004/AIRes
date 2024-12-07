using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chef : MonoBehaviour
{
    private Queue<Order> orderQueue; // Holds orders to be processed
    private Order currentOrder; // Current order being cooked
    private float cookingTime; // Time remaining to finish cooking
    private GameObject cookingDishInstance; // Current dish prefab being shown in the scene

    private enum State { Idle, Cooking, WaitingForOrder, MovingToPot }
    private State currentState;

    public Transform saladPot; // Reference to the Salad Pot
    public Transform burgerPot; // Reference to the Burger Pot
    public Transform pizzaPot; // Reference to the Pizza Pot

    private Transform targetPot; // Target pot to move to

    void Start()
    {
        orderQueue = new Queue<Order>();
        currentState = State.Idle;

        if (saladPot == null || burgerPot == null || pizzaPot == null)
        {
            Debug.LogError("One or more pot references are missing in the Inspector. Please assign them.");
        }
    }


    void Update()
    {
        UpdateState();  // Update the current state of the chef
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                if (orderQueue.Count > 0)
                {
                    // Process next order in the queue
                    currentOrder = orderQueue.Dequeue();
                    //cookingTime = currentOrder.GetPreparationTime();  // Set cooking time based on the order
                    currentState = State.MovingToPot;  // Chef needs to move to the pot
                    SetTargetPot(currentOrder);  // Set the correct pot to move to
                }
                break;

            case State.MovingToPot:
                MoveToPot();  // Move to the designated pot
                break;

            case State.Cooking:
                Cook();  // Continue cooking the current dish
                break;

            case State.WaitingForOrder:
                // Waiting for the next order
                break;
        }
    }

    public void ReceiveOrder(Order order)
    {
        if (order == null)
        {
            Debug.LogWarning("Received a null order.");
            return;
        }

        orderQueue.Enqueue(order);
        Debug.Log($"Chef received order for {order.DishName}. Added to the queue.");
    }


    // Method to set the correct target pot based on the order
    void SetTargetPot(Order order)
    {
        if (order == null)
        {
            Debug.LogError("Order is null. Cannot determine the target pot.");
            return;
        }

        switch (order.DishName)
        {
            case "Salad":
                targetPot = saladPot;
                break;
            case "Burger":
                targetPot = burgerPot;
                break;
            case "Pizza":
                targetPot = pizzaPot;
                break;
            default:
                Debug.LogError($"Unknown dish name: {order.DishName}. Cannot assign pot.");
                targetPot = null;
                break;
        }

        if (targetPot == null)
        {
            Debug.LogError("Target pot is not assigned in the Inspector or is invalid.");
        }
    }


    // Method to move the chef towards the target pot
    void MoveToPot()
    {
        if (targetPot == null)
        {
            Debug.LogError("Target pot not set! Ensure all pots are assigned in the Inspector.");
            currentState = State.Idle;
            return;
        }

        // Move the chef towards the target pot
        float step = 3f * Time.deltaTime; // Speed of movement
        transform.position = Vector3.MoveTowards(transform.position, targetPot.position, step);

        // When chef reaches the pot
        if (Vector3.Distance(transform.position, targetPot.position) < 0.1f)
        {
            currentState = State.Cooking;  // Start cooking once at the pot
            PrepareDish(currentOrder);  // Show the dish in the scene while cooking
        }
    }


    void PrepareDish(Order order)
    {
        if (order.DishPrefab == null)
        {
            Debug.LogError($"DishPrefab is missing for the order: {order.DishName}.");
            return;
        }

        if (cookingDishInstance != null)
        {
            Destroy(cookingDishInstance);
        }

        cookingDishInstance = Instantiate(order.DishPrefab, targetPot.position, Quaternion.identity);
    }


    void Cook()
    {
        cookingTime -= Time.deltaTime;  // Decrease cooking time

        if (cookingTime <= 0)
        {
            NotifyWaitStaff(currentOrder);  // Notify the waiter when the dish is ready
            Destroy(cookingDishInstance);  // Remove the dish from the scene
            currentState = State.Idle;  // Return to idle state after cooking
        }
    }

    // Method to notify wait staff when the dish is ready
    void NotifyWaitStaff(Order order)
    {
        // You can replace this with an actual notification system to inform the wait staff
        // For now, just log to the console for testing
        Debug.Log($"Chef finished cooking {order.DishName}. Notifying the WaitStaff.");
    }
}
