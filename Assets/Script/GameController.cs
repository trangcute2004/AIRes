using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    //ensure  1 game controlloer in game
    public static GameController instance;
    //list to store all existing table, order
    public List<Table> tables = new List<Table>();
    private List<Order> menu = new List<Order>();

    public WaitStaff WaitStaff;
    public Chef chef; 
    
    //spawn customer position
    public Vector3 spawnPosition; 
    //rate at which customer spawn
    public int customerSpawnRate;
    
    [SerializeField] public GameObject burgerPrefab;
    [SerializeField] public GameObject pizzaPrefab;
    [SerializeField] public GameObject saladPrefab;
    public GameObject customerPrefab;

    public float totalIncome = 0f;  // Track the total income
    public Text totalIncomeText;  // Reference to the text UI element to show the income
    public Text resultText;  // Reference to the text UI element to display WIN or LOSE
    public Text targetIncomeText;  // Reference to the text UI element to display the target income
    public GameObject paymentUI;  // Reference to the payment UI panel (optional)
    public float targetIncome = 0f;  // The target income the player needs to reach

    private bool hasGameEnded = false; // To ensure the game is checked only once after the countdown

    // this is called when the game start
    private void Awake()
    {
        //make globally accessible
        instance = this;
    }

    void Start()
    {
        //speed up the game 3 times
        Time.timeScale = 3.0f;
        // Generate a random target income (between $50 and $100)
        targetIncome = Random.Range(50f, 100f);

        //Check if the customerPrefab has been assigned in the inspector.
        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is missing. Please assign it in the inspector.");
            return;
        }

        InitializeTables();  // Initialize tables
        InitializeWaitStaff(); // Initialize wait staff
        //InitializeChef(); // Initialize chef (if needed)
        InitializeMenu(); // Initialize the menu with orders
        InvokeRepeating("SpawnCustomer", 0, 7f); // Spawn the first customer immediately, then every 7 seconds

        // Initialize the UI text
        totalIncomeText.text = $"Total Income: {totalIncome}$";  // Display the initial income
        resultText.gameObject.SetActive(false);  // Hide the result text at the beginning

        // Initialize the UI text
        totalIncomeText.text = $"Total Income: {totalIncome}$";  // Display the initial income
        targetIncomeText.text = $"Target Income: {targetIncome}$";  // Display the target income
        resultText.gameObject.SetActive(false);  // Hide the result text at the beginning
    }

    public void AddIncome(float amount)
    {
        totalIncome += amount;  // Add the amount to the total income
        totalIncomeText.text = $"Total Income: {totalIncome}$";  // Update the UI with the new income
        Debug.Log($"Total income: {totalIncome}$");

        // Check if the player has reached the target income
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        // Ensure this check only happens after the countdown timer is over
        if (!hasGameEnded)
        {
            if (totalIncome >= targetIncome)
            {
                resultText.gameObject.SetActive(true);  // Show the result text
                resultText.text = "WIN";  // Display WIN
                Debug.Log("WIN - Player has met the target income!");
            }
            else
            {
                resultText.gameObject.SetActive(true);  // Show the result text
                resultText.text = "LOSE";  // Display LOSE
                Debug.Log("LOSE - Player did not meet the target income.");
            }

            hasGameEnded = true; // Prevent multiple win/lose checks after the timer is over
        }
    }

    // Optionally, you can show the payment UI, but it's not necessary for automatic payment
    public void ShowPaymentUI(float amount)
    {
        paymentUI.SetActive(true);  // Activate the payment UI panel
        paymentUI.GetComponentInChildren<Text>().text = $"Pay: {amount}$";  // Set the amount to pay in the UI
    }

    //add food items to menu
    private void InitializeMenu()
    {
        menu.Clear(); // Ensure the menu starts empty

        // Check if the burger prefab exists in the scene
        if (burgerPrefab != null)
        {
            // Add a new order for a burger to the menu, with a price of 5.0f and a preparation time of 5 seconds.
            menu.Add(new Order("Burger", 5.0f, burgerPrefab, 5f, 5.0f));  
            Debug.Log("Added Burger to the menu.");
        }
        else
        {
            Debug.LogError("Burger prefab is missing.");
        }

        // Check if the salad prefab exists in the scene.
        if (saladPrefab != null)
        {
            //// Add a new order for a salad to the menu, with a price of 3.0f and a preparation time of 3 seconds.
            menu.Add(new Order("Salad", 3.0f, saladPrefab, 3f, 3.0f));  
            Debug.Log("Added Salad to the menu.");
        }
        else
        {
            Debug.LogError("Salad prefab is missing.");
        }

        // Check if the pizza prefab exists in the scene
        if (pizzaPrefab != null)
        {
            // Add a new order for a pizza to the menu, with a price of 10.0f and a preparation time of 8 seconds.
            menu.Add(new Order("Pizza", 10.0f, pizzaPrefab, 8f, 10.0f));  // 8 seconds for pizza
            Debug.Log("Added Pizza to the menu.");
        }
        else
        {
            Debug.LogError("Pizza prefab is missing.");
        }
    }

    // Initializes the tables in the game scene by finding all Table objects present.
    private void InitializeTables()
    {
        // Find all objects of type Table in the scene and convert them into a list.
        tables = new List<Table>(FindObjectsOfType<Table>());
    }

    /*private void InitializeChef()
    {
        chef = FindObjectOfType<Chef>();
    }*/

    //initializes the WaitStaff by finding the WaitStaff object in the scene.
    private void InitializeWaitStaff()
    {
        //// Find the first object of type WaitStaff in the scene.
        WaitStaff = FindObjectOfType<WaitStaff>();
    }

    // spawn a new customer in the game at the design spawn position.
    private void SpawnCustomer()
    {
        // Check if the customer prefab is assigned in the inspector.
        if (customerPrefab == null)
        {
            Debug.LogError("Customer prefab is not assigned!");
            return;
        }

        // Instantiate a new customer
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);

        // Get the Customer component attached to the instantiated customer object.
        Customer customer = customerObject.GetComponent<Customer>();

        // Check if the Customer component was found on the instantiated object.
        if (customer == null)
        {
            Debug.LogError("Spawned object does not have a Customer component.");
            return; // Exit the method if the `Customer` component is not found.
        }

        // Debugging check to ensure the menu is properly initialized and not empty.
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

