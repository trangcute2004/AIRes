using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//FINITE STATE MACHINE
public enum DATA_MENU {Salad, Burger, Pizza}
[System.Serializable]
public class ItemMenu
{
    public DATA_MENU typeFood;
    public int price;
    public float timeToMakeFood;
}
public class RestaurantController : MonoBehaviour
{
    //name of restaurant
    public string nameRestau;

    // List of tables in the restaurant
    [SerializeField] List<TableController> tables;

    public Door door;
    // Menu items available at the restaurant
    [SerializeField] List<ItemMenu> menus;

    // Max number of customers allowed in the queue at once
    [SerializeField] int maxQueuCus = 5;

    // List of customers currently in the queue
    List<CustomerController> queueCus;

    // Wait staff reference and their position
    [SerializeField] WaitStaffController waitStaff;
    [SerializeField] Transform transWaistaff;

    // List of tables currently reserved in the queue
    public List<TableController> queueTables = new List<TableController>();

    // The current coin of the restaurant
    public int coin { get; private set; }

    [SerializeField] TextMeshProUGUI txtName;
    [SerializeField] TextMeshProUGUI txtDes;

    void Start()
    {
        // Create the restaurant with a coin balance of 0
        SetCoin(0);

        // Create the queue of customers
        queueCus = new List<CustomerController>();

        // Set the restaurant name in the UI
        txtName.text = nameRestau;

        // Create the wait staff with the current position and restaurant reference
        waitStaff.Init(transWaistaff, this);
    }

    // Updates the description text
    void UpdateDes()
    {
        txtDes.text = "$" + coin;
    }

    // Check if the restaurant has any available tables
    public bool IsFull()
    {
        return GetTableFree() != null;
    }

    // Get a free table from the list of tables in the restaurant
    public TableController GetTableFree()
    {
        // Loop through all tables and return the first one that is empty
        for (int i = 0; i < tables.Count; i++)
            if (tables[i].stateTable == STATE_TABLE.Empty)
                return tables[i];
        return null;
    }

    // Return the length of the current queue (number of customers waiting)
    public int GetLengthQueue() => queueCus.Count;

    // Add a customer to the queue if there is space
    public void AddQueue(CustomerController customerController)
    {
        // If the queue is full, remove the customer by calling Vote with false (not appreciated)
        if (queueCus.Count == maxQueuCus)
        {
            Vote(customerController, false);
            Destroy(customerController);
        }
        else
        {
            // Otherwise, add the customer to the queue
            queueCus.Add(customerController);
        }
    }

    // Remove a customer from the queue when they leave
    public void LeaveQueue(CustomerController customerController)
    {
        // Call Vote with false, indicating the customer didn't appreciate the service
        Vote(customerController, false);

        // Remove the customer from the queue
        queueCus.Remove(customerController);
    }

    // The customer places an order for food
    public void Oder(CustomerController customerController, DATA_MENU food)
    {
        // Pass the food order to the customer's table controller
        customerController.tableController.Oder(food);

        // Add the table to the list of reserved tables
        queueTables.Add(customerController.tableController);
    }

    // Process the payment when the customer finishes eating
    public void Pay(CustomerController customerController, TableController table)
    {
        // Find the item in the menu based on the food ordered at the table
        var item = FindItem(table.food);

        // Apply a discount to the cooking time for the food (e.g., faster service)
        item.timeToMakeFood *= 0.95f;

        // Increase the restaurant's coin balance by the price of the food
        SetCoin(coin + item.price);

        // Call the Vote method with true, indicating the customer appreciated the service
        Vote(customerController, true);
    }

    // Vote for the customer based on whether they appreciated the service
    public void Vote(CustomerController customerController, bool isAppreciate)
    {
        // If the customer has a table, clear the table (i.e., they are done)
        if (customerController.tableController != null)
        {
            customerController.tableController.ClearTable();
            // If the customer was in the queue, remove them from the queue
            if (queueCus.Count > 0)
            {
                queueCus[0].OutQueue(customerController.tableController);
                queueCus.RemoveAt(0);
            }

            // If the customer’s table is in the reserved tables list, remove it
            if (queueTables.Contains(customerController.tableController))
            {
                queueTables.Remove(customerController.tableController);
            }
        }

        // If the customer didn't appreciate the service, reduce the restaurant's coin balance by 2
        if (!isAppreciate)
        {
            SetCoin(coin - 2);
        }
    }

    // Check if the customer can afford to buy any items from the menu based on their coin balance
    public bool CanBuyItem(float coin)
    {
        return ItemCanBuy(coin).Count > 0;
    }

    // Get a list of items that the customer can afford based on their coin balance
    public List<ItemMenu> ItemCanBuy(float coin)
    {
        List<ItemMenu> result = new List<ItemMenu>();

        // Loop through the menu items and add the ones that the customer can afford
        for (int i = 0; i < menus.Count; i++)
            if (menus[i].price <= coin)
                result.Add(menus[i]);
        return result;
    }

    // Find an item from the menu based on the food type ordered by the customer
    public ItemMenu FindItem(DATA_MENU food)
    {
        return menus.Find(x => x.typeFood == food);
    }

    // Set the coin balance of the restaurant, ensuring it stays within the range [0, 9999]
    void SetCoin(int value)
    {
        coin = Mathf.Clamp(value, 0, 9999);

        // Update the description text with the new coin balance
        UpdateDes();
    }
}
