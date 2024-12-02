using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public List<Table> tables = new List<Table>(); // List to store all existing tables in the scene
    public WaitStaff WaitStaff;
    public Chef chef; // Chef reference

    public GameObject customerPrefab; // Customer prefab
    public Vector3 spawnPosition; // Position where customers will spawn
    public int customerSpawnRate; // Rate at which customers spawn
    [SerializeField] public GameObject burgerPrefab;
    [SerializeField] public GameObject pizzaPrefab;
    [SerializeField] public GameObject saladPrefab;

    private List<Order> menu = new List<Order>(); // Declare the menu list here

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 3.0f;
        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is missing. Please assign it in the inspector.");
            return;
        }

        InitializeTables();  // Initialize tables (they should already exist in the scene)
        InitializeWaitStaff(); // Initialize wait staff (if needed)
        InitializeChef(); // Initialize chef (if needed)
        InitializeMenu(); // Initialize the menu with orders
        InvokeRepeating("SpawnCustomer", 0, 5f); // Spawn the first customer immediately, then every 5 seconds

    }

    private void InitializeMenu()
    {
        menu.Clear(); // Ensure the menu starts empty

        if (burgerPrefab != null)
        {
            menu.Add(new Order("Burger", 5.0f, burgerPrefab));
            Debug.Log("Added Burger to the menu.");
        }
        else
        {
            Debug.LogError("Burger prefab is missing.");
        }

        if (saladPrefab != null)
        {
            menu.Add(new Order("Salad", 3.0f, saladPrefab));
            Debug.Log("Added Salad to the menu.");
        }
        else
        {
            Debug.LogError("Salad prefab is missing.");
        }

        if (pizzaPrefab != null)
        {
            menu.Add(new Order("Pizza", 10.0f, pizzaPrefab));
            Debug.Log("Added Pizza to the menu.");
        }
        else
        {
            Debug.LogError("Pizza prefab is missing.");
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
        WaitStaff = FindObjectOfType<WaitStaff>();
    }
    private void Update()
    {
       
    }
    private void SpawnCustomer()
    {
        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is not assigned!");
            return;
        }

        // Instantiate a new customer
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        if (customer == null)
        {
            Debug.LogError("Spawned object does not have a Customer component.");
            return;
        }

        // Debug the menu content
        if (menu == null || menu.Count == 0)
        {
            Debug.LogError("Menu is not initialized or empty. Ensure it is populated in GameController.");
            return;
        }

        // Assign the menu to the customer
        Debug.Log($"Assigning menu with {menu.Count} items to customer {customerObject.name}.");
        customer.SetMenu(menu);

        // Assign a table to the customer
        customer.FindTable(tables);
    }

}

