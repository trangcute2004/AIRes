using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<Table> tables = new List<Table>(); // List to store all existing tables in the scene
    public List<WaitStaff> waitStaff; // Wait staff list
    public Chef chef; // Chef reference
    public GameObject customerPrefab; // Customer prefab
    public Vector3 spawnPosition; // Position where customers will spawn
    public int customerSpawnRate; // Rate at which customers spawn
    public GameObject burgerPrefab;  // Assign this in the inspector for the burger sprite
    public GameObject pizzaPrefab;   // Assign this in the inspector for the pizza sprite
    public GameObject saladPrefab;   // Assign this in the inspector for the salad sprite

    private List<Order> menu = new List<Order>(); // Declare the menu list here

    void Start()
    {
        InitializeTables();  // Initialize tables (they should already exist in the scene)
        InitializeWaitStaff(); // Initialize wait staff (if needed)
        InitializeChef(); // Initialize chef (if needed)
        InitializeMenu(); // Initialize the menu with orders
        InvokeRepeating("SpawnCustomer", 0, customerSpawnRate); // Repeatedly spawn customers
    }

    // Initialize the menu items
    private void InitializeMenu()
    {
        if (burgerPrefab == null || pizzaPrefab == null || saladPrefab == null)
        {
            Debug.LogError("Dish Prefabs are not assigned!");
        }

        // Adding 3 order items to the menu with Sprite assigned
        menu.Add(new Order("Burger", 5.0f, burgerPrefab));  // 5 seconds preparation time
        menu.Add(new Order("Pizza", 10.0f, pizzaPrefab));   // 10 seconds preparation time
        menu.Add(new Order("Salad", 3.0f, saladPrefab));    // 3 seconds preparation time
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
        // You can spawn at any corner. For example, let's spawn at the top-left corner:
        Vector3 spawnPosition = new Vector3(-10, 0, 10); // Adjust these values based on your scene

        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        customer.SetMenu(menu); // Pass the available menu to the customer

        // Ensure the customer finds an available table from the list of tables
        customer.FindTable(tables); // This will handle moving to the table
    }



    void Update()
    {
        // Here you can update other game elements like state management for customers, chefs, etc.
        // For instance, updating state for WaitStaff, Chef, and Customers
        // foreach (var waiter in waitStaff) waiter.UpdateState();
        // chef.UpdateState();
    }
}
