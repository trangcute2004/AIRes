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

        // Clear the menu to prevent duplicate entries
        menu.Clear();

        // Validate and add burger
        if (burgerPrefab == null)
        {
            Debug.LogError("Burger prefab is not assigned in the Inspector!");
        }
        else
        {
            Debug.Log($"Burger prefab detected: {burgerPrefab.name}");
            menu.Add(new Order("Burger", 5.0f, burgerPrefab));
        }

        // Validate and add pizza
        if (pizzaPrefab == null)
        {
            Debug.LogError("Pizza prefab is not assigned in the Inspector!");
        }
        else
        {
            Debug.Log($"Pizza prefab detected: {pizzaPrefab.name}");
            menu.Add(new Order("Pizza", 10.0f, pizzaPrefab));
        }

        // Validate and add salad
        if (saladPrefab == null)
        {
            Debug.LogError("Salad prefab is not assigned in the Inspector!");
        }
        else
        {
            Debug.Log($"Salad prefab detected: {saladPrefab.name}");
            menu.Add(new Order("Salad", 3.0f, saladPrefab));
        }

        // Log final menu
        Debug.Log($"Menu initialized with {menu.Count} items.");
        foreach (var order in menu)
        {
            Debug.Log($"Order added to menu: {order.DishName}, Prefab: {order.DishPrefab?.name ?? "null"}");
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

        if (menu == null || menu.Count == 0)
        {
            Debug.LogError("Menu is not initialized or is empty. Ensure InitializeMenu is called first.");
            return;
        }

        customer.SetMenu(menu);
        customer.FindTable(tables);

        Debug.Log($"Customer {customer.gameObject.name} spawned and assigned a menu with {menu.Count} items.");
    }
}
