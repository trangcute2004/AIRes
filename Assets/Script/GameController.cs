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
    [SerializeField] private GameObject burgerPrefab;
    [SerializeField] private GameObject pizzaPrefab;
    [SerializeField] private GameObject saladPrefab;

    private List<Order> menu = new List<Order>(); // Declare the menu list here

    void Start()
    {
        Debug.Log($"Burger Prefab: {burgerPrefab?.name}");
        Debug.Log($"Pizza Prefab: {pizzaPrefab?.name}");
        Debug.Log($"Salad Prefab: {saladPrefab?.name}");

        InitializeTables();  // Initialize tables (they should already exist in the scene)
        InitializeWaitStaff(); // Initialize wait staff (if needed)
        InitializeChef(); // Initialize chef (if needed)
        InitializeMenu(); // Initialize the menu with orders
        InvokeRepeating("SpawnCustomer", 0, customerSpawnRate); // Repeatedly spawn customers
    }

    private void InitializeMenu()
    {
        Debug.Log("Initializing menu...");
        menu.Clear(); // Ensure the menu starts empty

        // Add burger order
        if (burgerPrefab != null)
        {
            var burgerOrder = new Order("Burger", 5.0f, burgerPrefab);
            menu.Add(burgerOrder);
            Debug.Log($"Order created: {burgerOrder.DishName}, Prefab: {burgerOrder.DishPrefab.name}");
        }
        else
        {
            Debug.LogWarning("Burger prefab is not assigned!");
        }

        // Add pizza order
        if (pizzaPrefab != null)
        {
            var pizzaOrder = new Order("Pizza", 10.0f, pizzaPrefab);
            menu.Add(pizzaOrder);
            Debug.Log($"Order created: {pizzaOrder.DishName}, Prefab: {pizzaOrder.DishPrefab.name}");
        }
        else
        {
            Debug.LogWarning("Pizza prefab is not assigned!");
        }

        // Add salad order
        if (saladPrefab != null)
        {
            var saladOrder = new Order("Salad", 3.0f, saladPrefab);
            menu.Add(saladOrder);
            Debug.Log($"Order created: {saladOrder.DishName}, Prefab: {saladOrder.DishPrefab.name}");
        }
        else
        {
            Debug.LogWarning("Salad prefab is not assigned!");
        }

        Debug.Log($"Menu initialized with {menu.Count} items.");
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

        if (menu == null || menu.Count == 0)
        {
            Debug.LogError("Menu is not initialized or is empty. Ensure InitializeMenu is called first.");
            return;
        }

        customer.SetMenu(menu); // Pass the menu to the customer
        customer.FindTable(tables); // Assign a table to the customer

        Debug.Log($"Customer {customer.gameObject.name} spawned and assigned a menu with {menu.Count} items.");
    }
}
