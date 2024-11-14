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
    [SerializeField] public GameObject burgerPrefab;
    [SerializeField] public GameObject pizzaPrefab;
    [SerializeField] public GameObject saladPrefab;

    private List<Order> menu = new List<Order>(); // Declare the menu list here

    void Start()
    {
        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is missing. Please assign it in the inspector.");
            return;
        }

        InitializeTables();  // Initialize tables (they should already exist in the scene)
        InitializeWaitStaff(); // Initialize wait staff (if needed)
        InitializeChef(); // Initialize chef (if needed)
        InitializeMenu(); // Initialize the menu with orders
        InvokeRepeating("SpawnCustomer", 0, customerSpawnRate); // Repeatedly spawn customers
    }

    private void InitializeMenu()
    {
        Debug.Log("Initializing menu...");
        menu.Clear(); // Ensure the menu is empty before adding new items

        // Add Burger
        if (burgerPrefab != null)
        {
            menu.Add(new Order("Burger", 5.0f, burgerPrefab));
            Debug.Log($"Added Burger to menu. Prefab: {burgerPrefab.name}");
        }
        else
        {
            Debug.LogError("Burger prefab is missing! Check GameController Inspector.");
        }

        // Add Pizza
        if (pizzaPrefab != null)
        {
            menu.Add(new Order("Pizza", 10.0f, pizzaPrefab));
            Debug.Log($"Added Pizza to menu. Prefab: {pizzaPrefab.name}");
        }
        else
        {
            Debug.LogError("Pizza prefab is missing! Check GameController Inspector.");
        }

        // Add Salad
        if (saladPrefab != null)
        {
            menu.Add(new Order("Salad", 3.0f, saladPrefab));
            Debug.Log($"Added Salad to menu. Prefab: {saladPrefab.name}");
        }
        else
        {
            Debug.LogError("Salad prefab is missing! Check GameController Inspector.");
        }

        if (menu.Count == 0)
        {
            Debug.LogError("Menu initialization failed. No valid items were added. Check prefab assignments.");
        }
        else
        {
            Debug.Log($"Menu initialized successfully with {menu.Count} items.");
        }
    }


    private void InitializeTables()
    {
        tables = new List<Table>(FindObjectsOfType<Table>());
    }

    private void InitializeChef()
    {
        chef = FindObjectOfType<Chef>();
    }

    private void InitializeWaitStaff()
    {
        waitStaff = new List<WaitStaff>(FindObjectsOfType<WaitStaff>());
        foreach (var waiter in waitStaff)
        {
            waiter.chefLocation = chef?.transform;
        }
    }

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

        // Validate the menu
        Debug.Log("Validating menu before passing to customer...");
        List<Order> validMenu = new List<Order>();
        foreach (var order in menu)
        {
            if (order == null || order.DishPrefab == null)
            {
                Debug.LogError($"Invalid order in menu. DishPrefab is null for order {order?.DishName ?? "null"}. Skipping this order.");
                continue;
            }
            validMenu.Add(order);
        }

        if (validMenu.Count == 0)
        {
            Debug.LogError("No valid orders available in the menu. Cannot assign menu to customer.");
            return;
        }

        customer.SetMenu(validMenu); // Pass the validated menu to the customer
        customer.FindTable(tables); // Assign a table to the customer

        Debug.Log($"Customer {customer.gameObject.name} spawned and assigned a menu with {validMenu.Count} items.");
    }


}
