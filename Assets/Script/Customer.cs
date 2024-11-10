using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum State { LookingForTable, WaitingForOrder, Eating, Leaving }
    private State currentState;
    private Table assignedTable;

    public void FindTable(List<Table> tables)
    {
        // Find an available table
        assignedTable = tables.Find(t => !t.IsOccupied && !t.IsDirty);
        assignedTable.Occupy();
        currentState = State.WaitingForOrder;
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case State.LookingForTable:
                // Already handled in FindTable
                break;
            case State.WaitingForOrder:
                // Wait for wait staff
                break;
            case State.Eating:
                Eat();
                break;
            case State.Leaving:
                Leave();
                break;
        }
    }

    void Eat()
    {
        // Simulate eating with a timer
        currentState = State.Leaving;
    }

    void Leave()
    {
        assignedTable.Vacate();
        Destroy(gameObject); // Remove customer from game
    }
}
