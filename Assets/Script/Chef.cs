using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chef : MonoBehaviour
{
    private Queue<Order> orderQueue;
    private Order currentOrder;
    private float cookingTime;

    private enum State { Idle, Cooking, WaitingForOrder }
    private State currentState;

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                if (orderQueue.Count > 0)
                {
                    currentOrder = orderQueue.Dequeue();
                    cookingTime = currentOrder.GetPreparationTime();
                    currentState = State.Cooking;
                }
                break;
            case State.Cooking:
                Cook();
                break;
            case State.WaitingForOrder:
                // Wait for next order
                break;
        }
    }

    void Cook()
    {
        cookingTime -= Time.deltaTime;
        if (cookingTime <= 0)
        {
            NotifyWaitStaff(currentOrder);
            currentState = State.Idle;
        }
    }

    public void ReceiveOrder(Order order)
    {
        orderQueue.Enqueue(order);
    }

    void NotifyWaitStaff(Order order)
    {
        // Notify wait staff that the order is ready
    }
}

