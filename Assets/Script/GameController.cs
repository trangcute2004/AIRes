using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<Table> tables;
    public List<WaitStaff> waitStaff;
    public Chef chef;
    public List<Customer> customers = new List<Customer>();
    public GameObject customerPrefab;  // Prefab for spawning customers
    public Vector3 spawnPosition;      // Set this to the position where customers should appear
    public int customerSpawnRate;

    void Start()
    {
        InitializeTables();
        InitializeWaitStaff();
        InitializeChef();
        InvokeRepeating("SpawnCustomer", 0, customerSpawnRate);
    }

    private void InitializeChef()
    {
        chef = FindObjectOfType<Chef>(); // Assume only one chef in the scene
    }

    private void InitializeWaitStaff()
    {
        waitStaff = new List<WaitStaff>(FindObjectsOfType<WaitStaff>()); // Find all WaitStaff in the scene
    }

    private void InitializeTables()
    {
        tables = new List<Table>(FindObjectsOfType<Table>()); // Assume each table is a GameObject with the Table script attached
    }

    void SpawnCustomer()
    {
        // Instantiate a new customer GameObject at the spawn position
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);

        // Get the Customer component
        Customer customer = customerObject.GetComponent<Customer>();

        // Ensure the customer finds a table and add it to the list of customers
        customer.FindTable(tables);
        customers.Add(customer);
    }

    void Update()
    {
        //foreach (var waiter in waitStaff) waiter.UpdateState();
        //chef.UpdateState();
        //foreach (var customer in customers) customer.UpdateState();
    }
}