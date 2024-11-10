using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitStaff : MonoBehaviour
{
    private enum State { Idle, Cleaning, Serving, TakingOrder }
    private State currentState;
    private Table targetTable;

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                FindNextTask();
                break;
            case State.Cleaning:
                CleanTable();
                break;
            case State.Serving:
                ServeOrder();
                break;
            case State.TakingOrder:
                TakeOrder();
                break;
        }
    }

    void FindNextTask()
    {
        // Check for urgent tasks like serving ready dishes
        // Switch state based on priority
    }

    void CleanTable()
    {
        // Code to clean the table
        targetTable.Clean();
        currentState = State.Idle;
    }

    void ServeOrder()
    {
        // Code to serve food to a customer
        currentState = State.Idle;
    }

    void TakeOrder()
    {
        // Code to take an order from a customer
        currentState = State.Idle;
    }
}
