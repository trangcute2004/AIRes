using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<Table> tables = new List<Table>(); // List to store all existing tables in the scene
    public List<WaitStaff> waitStaff; // Wait staff list
    public Chef chef; // Chef reference
    public GameObject customerPrefab; // Customer prefab
    public Vector3 spawnPosition; // Position where customers will spawn
    public int customerSpawnRate; // Rate at which customers spawn

    private List<Order> menu = new List<Order>(); // Declare the menu list here

    void Start()
    {
        InitializeTables();  // Initialize tables (they should already exist in the scene)
        InitializeWaitStaff(); // Initialize wait staff (if needed)
        InitializeChef(); // Initialize chef (if needed)
        InitializeMenu(); // Initialize the menu with orders
        InvokeRepeating("SpawnCustomer", 0, customerSpawnRate); // Repeatedly spawn customers
    }

    private void InitializeMenu()
    {
        // Adding 3 order items to the menu
        menu.Add(new Order("Burger", 5.0f));  // 5 seconds preparation time
        menu.Add(new Order("Pizza", 10.0f));  // 10 seconds preparation time
        menu.Add(new Order("Salad", 3.0f));   // 3 seconds preparation time
    }

    // Initialize existing tables (find all Table objects in the scene)
    private void InitializeTables()
    {
        // Find all tables in the scene and add them to the tables list
        tables = new List<Table>(FindObjectsOfType<Table>());
    }

    // Method to initialize the chef (optional)
    private void InitializeChef()
    {
        chef = FindObjectOfType<Chef>(); // Assuming there's one chef in the scene
    }

    // Method to initialize the wait staff (optional)
    private void InitializeWaitStaff()
    {
        waitStaff = new List<WaitStaff>(FindObjectsOfType<WaitStaff>()); // Find all WaitStaff in the scene
    }

    // Method to spawn a new customer at the spawn position
    void SpawnCustomer()
    {
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        customer.SetMenu(menu); // Set the menu with orders for the customer

        // Ensure the customer finds an available table from the list of tables
        customer.FindTable(tables); // Pass the list of existing tables to find an empty one
    }

    void Update()
    {
        // You can update the state for WaitStaff, Chef, and Customers here if needed
        // For example, update customer states or interactions
        // foreach (var waiter in waitStaff) waiter.UpdateState();
        // chef.UpdateState();
        // foreach (var customer in customers) customer.UpdateState();
    }
}
