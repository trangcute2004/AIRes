using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//FINITE STATE MACHINE
public enum STATE_TABLE { Empty, Reserved, Ordered, Served}

public class TableController : MonoBehaviour
{
    public Transform standdingWaitStaff; // Position of the wait staff at the table
    public Transform standingFood;      // Position of the food at the table
    public Transform standingCus;       // Position of the customer at the table


    // The current state of the table (Empty, Reserved, Ordered, or Served)
    public STATE_TABLE stateTable { get; private set; }

    // The food item that the customer has ordered
    public DATA_MENU food { get; private set; }

    // A reference to the restaurant controller, which manages this table
    RestaurantController restaurantController;

    // A reference to the customer who is sitting at the table
    CustomerController customer;

    private void Start()
    {
        stateTable = STATE_TABLE.Empty;
    }

    // create the table with the restaurant controller
    public void Init(RestaurantController restaurantController)
    {
        // Set the reference to the restaurant controller
        this.restaurantController = restaurantController;
    }

    // Reserve the table for a customer
    public void ReserveTable(CustomerController customer)
    {
        // Set the customer for the table
        this.customer = customer;
        stateTable = STATE_TABLE.Reserved;
    }

    // Check if the table is still reserved by a customer. If not, set the table back to Empty.
    public void CheckStateTable()
    {
        // If there's no customer at the table, mark the table as empty
        if (customer == null)
            stateTable = STATE_TABLE.Empty;
    }

    // The customer places an order for food
    public void Oder(DATA_MENU food)
    {
        // Set the food item ordered by the customer
        this.food = food;
        stateTable = STATE_TABLE.Ordered;
    }

    // Mark the table as served once the food is ready and delivered to the customer
    public void MarkAsServed()
    {
        // Set the customer's state to "EatFood" once they are served
        customer.SetState(CustomerController.STATE_CUSTOMER.EatFood);

        stateTable = STATE_TABLE.Served;

        // Enable the food sprite at the table to show that it's served
        standingFood.gameObject.SetActive(true);

        // Set the food sprite based on the ordered food
        standingFood.GetComponent<SpriteRenderer>().sprite = GamePlayManager.instance.spriteCooks[(int)food];
    }

    // Clear the table after the customer has finished eating
    public void ClearTable()
    {
        // Reset the food item to default (no food)
        food = default;

        // Hide the food sprite from the table
        standingFood.gameObject.SetActive(false);

        // Set the table state back to Empty, indicating the table is available
        stateTable = STATE_TABLE.Empty;
    }
}
