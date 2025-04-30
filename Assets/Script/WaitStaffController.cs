using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//FINITE STATE MACHINE
public enum STATE_WAITSTAFF {Idle, Cooking, MoveToServe, MoveToIdle}


public class WaitStaffController : MonoBehaviour
{
    // Wait staff's skill level (affects how fast they cook and serve)
    public float skillLevel = 3f;

    // Wait staff's movement speed
    public float speed = 2f;

    // Private variable to store the current state of the wait staff
    private STATE_WAITSTAFF state = STATE_WAITSTAFF.Idle;

    // The time remaining for cooking before the food is ready to serve
    private float cookingTimeLeft = 0f;

    // A reference to the restaurant controller that manages this staff
    private RestaurantController restaurantController;

    // The position where the wait staff should return when idle
    private Transform restPosition;
    //private SpriteRenderer skin;


    // Create the wait staff with a restaurant controller and position
    public void Init(Transform trans, RestaurantController restaurantController)
    {
        // Set the first state to Idle
        SetState(STATE_WAITSTAFF.Idle);

        // Set the rest position for when the staff is idle
        restPosition = trans;

        // Set the restaurant controller reference
        this.restaurantController = restaurantController;
    }

    void Update()
    {
        switch (state)
        {
            case STATE_WAITSTAFF.Idle:
                HandleIdle();
                break;

            case STATE_WAITSTAFF.Cooking:
                HandleCooking();
                break;

            case STATE_WAITSTAFF.MoveToServe:
                // If there are no tables in the queue, transition to idle state
                if (restaurantController.queueTables.Count == 0)
                {
                    SetState(STATE_WAITSTAFF.MoveToIdle);
                    break;
                }

                // Move towards the first table in the queue to serve the food
                MoveTo(restaurantController.queueTables[0].standdingWaitStaff.position, () =>
                {
                    // Once the staff reaches the table, serve the food
                    restaurantController.queueTables[0].MarkAsServed();

                    // Remove the served table from the queue
                    restaurantController.queueTables.RemoveAt(0);
                    SetState(STATE_WAITSTAFF.MoveToIdle);
                });
                break;
            case STATE_WAITSTAFF.MoveToIdle:

                // Move back to the rest position when idle
                MoveTo(restPosition.position, () => SetState(STATE_WAITSTAFF.Idle));
                break;
        }
    }

    // Handles the logic when the wait staff is in the Idle state
    void HandleIdle()
    {
        if (restaurantController.queueTables.Count != 0)
        {
            // If there are tables in the queue, start cooking
            SetState(STATE_WAITSTAFF.Cooking);

            // Get the food ordered by the first customer in the queue
            DATA_MENU food = restaurantController.queueTables[0].food;

            // Set the cooking time for the food
            cookingTimeLeft = AdjustCookingTime(restaurantController.FindItem(food).timeToMakeFood);
        }
    }

    // Handles the logic when the wait staff is in the Cooking state
    void HandleCooking()
    {
        // If cooking time is over, transition to MoveToServe state to serve the food
        if (cookingTimeLeft <= 0)
        {
            Debug.Log("Chef has finished cooking the food");

            // Increase the wait staff's skill level slightly each time they cook
            skillLevel *= 1.05f;
            SetState(STATE_WAITSTAFF.MoveToServe);
        }
        else
        {
            // Decrease the cooking time over time as the food cooks
            cookingTimeLeft -= Time.deltaTime;
        }
    }

    // Moves the wait staff towards a target position
    void MoveTo(Vector2 target, System.Action onArrive)
    {
        // Move the wait staff towards the target position at the defined speed
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // If the wait staff is close enough to the target, invoke the onArrive action
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            // Call the action that happens when the target is reached
            onArrive.Invoke();
        }
    }

    // Adjusts the cooking time based on the wait staff's skill level
    float AdjustCookingTime(float baseTime)
    {
        // If the staff's skill level is high, reduce cooking time
        if (skillLevel >= 3f)
            return baseTime * (1f - 0.05f * (skillLevel - 2f)); // Decrease time slightly based on skill level

        // If the skill level is lower, increase cooking time
        else
            return baseTime * (1f + 0.07f + (2f - skillLevel) * 0.06f); // Increase time based on skill level
    }

    // Increase time based on skill level
    CustomerController FindCustomerAtTable(TableController table)
    {

        // Find all customers in the scene and check if they are at the given table's position
        var customers = FindObjectsOfType<CustomerController>();
        foreach (var c in customers)
        {
            if (Vector2.Distance(c.transform.position, table.standingCus.position) < 0.2f)
                return c; // Return the customer at the table
        }
        return null;
    }

    // Sets the current state of the wait staff
    void SetState(STATE_WAITSTAFF s)
    {
        // Update the current state
        state = s;
    }
}
