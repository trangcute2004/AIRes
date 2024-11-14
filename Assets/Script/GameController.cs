
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
        Debug.Log("Initializing menu...");

        if (burgerPrefab == null)
        {
            Debug.LogError("Burger prefab is not assigned in the inspector!");
        }
        else
        {
            Debug.Log("Burger prefab assigned successfully.");
        }

        if (pizzaPrefab == null)
        {
            Debug.LogError("Pizza prefab is not assigned in the inspector!");
        }
        else
        {
            Debug.Log("Pizza prefab assigned successfully.");
        }

        if (saladPrefab == null)
        {
            Debug.LogError("Salad prefab is not assigned in the inspector!");
        }
        else
        {
            Debug.Log("Salad prefab assigned successfully.");
        }

        // Initialize the menu
        menu = new List<Order>();

        if (burgerPrefab != null)
            menu.Add(new Order("Burger", 5.0f, burgerPrefab));
        if (pizzaPrefab != null)
            menu.Add(new Order("Pizza", 10.0f, pizzaPrefab));
        if (saladPrefab != null)
            menu.Add(new Order("Salad", 3.0f, saladPrefab));

        foreach (var order in menu)
        {
            if (order == null || order.DishPrefab == null)
            {
                Debug.LogError($"Invalid order detected in the menu: {order?.DishName ?? "null"}");
            }
            else
            {
                Debug.Log($"Order added to menu: {order.DishName}");
            }
        }

        Debug.Log($"Menu initialized with {menu.Count} items.");
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
        waitStaff = new List<WaitStaff>(FindObjectsOfType<WaitStaff>());

        foreach (var waiter in waitStaff)
        {
            waiter.chefLocation = chef.transform; // Assign chef location
        }
    }


    // Method to spawn a new customer at the spawn position
    private void SpawnCustomer()
    {
        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is not assigned!");
            return;
        }

        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        if (customer == null)
        {
            Debug.LogError("Spawned object does not have a Customer component.");
            return;
        }

        if (menu == null || menu.Count == 0)
        {
            Debug.LogError("Menu is not initialized or is empty. Ensure InitializeMenu is called first.");
            return;
        }

        customer.SetMenu(menu); // Pass the menu to the customer
        customer.FindTable(tables); // Assign a table to the customer

        Debug.Log($"Customer {customer.gameObject.name} spawned and assigned a menu with {menu.Count} items.");
    }

    WaitStaff FindAvailableWaitStaff()
    {
        foreach (var waiter in waitStaff)
        {
            if (waiter.IsIdle()) // Implement IsIdle in WaitStaff
            {
                return waiter;
            }
        }
        Debug.LogWarning("No available waitstaff found!");
        return null;
    }

    void Update()
    {
        // Here you can update other game elements like state management for customers, chefs, etc.
        // For instance, updating state for WaitStaff, Chef, and Customers
    }



}
