using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject tablePrefab;
    public List<Table> tables = new List<Table>();
    public List<WaitStaff> waitStaff;
    public Chef chef;
    public GameObject customerPrefab;
    public Vector3 spawnPosition;
    public int customerSpawnRate;

    void Start()
    {
        InitializeTables();
        InitializeWaitStaff();
        InitializeChef();
        InvokeRepeating("SpawnCustomer", 0, customerSpawnRate);
    }

    private void InitializeTables()
    {
        // Ensure the tables list is cleared before initializing new tables
        tables.Clear();

        // Instantiate 6 tables
        for (int i = 0; i < 6; i++)
        {
            // Instantiate tablePrefab at a position on the screen, spaced evenly
            GameObject tableObject = Instantiate(tablePrefab, new Vector3(i * 2.0f, 0, 0), Quaternion.identity); // Spread tables on screen

            // Ensure the tablePrefab has the Table script attached
            Table table = tableObject.GetComponent<Table>();

            // Initialize each table with a unique table number (i+1) and a capacity of 4 customers
            table.InitializeTable(i + 1, 4);

            // Add the table to the tables list
            tables.Add(table);

            // Optionally, you can name the GameObject for easier debugging
            tableObject.name = $"Table {i + 1}";
        }
    }

    private void InitializeChef()
    {
        // Implement as needed
    }

    private void InitializeWaitStaff()
    {
        // Implement as needed
    }

    void SpawnCustomer()
    {
        GameObject customerObject = Instantiate(customerPrefab, spawnPosition, Quaternion.identity);
        Customer customer = customerObject.GetComponent<Customer>();

        customer.FindTable(tables); // Pass the list of tables to find an empty one
    }

    void Update()
    {
        //foreach (var waiter in waitStaff) waiter.UpdateState();
        //chef.UpdateState();
        //foreach (var customer in customers) customer.UpdateState();
    }
}