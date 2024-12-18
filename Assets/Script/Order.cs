using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Order
{
    //The name of the dish
    public string DishName { get; private set; }

    //The time required to prepare the dish
    public float PreparationTime { get; private set; }

    public GameObject DishPrefab { get; set; }

    //Tracks if the order has been delivered to the customer
    public bool IsDelivered = false;

    // The amount of time it takes for the customer to eat the dish
    public float EatingDuration { get; private set; }

    //initialize an order
    public Order(string dishName, float preparationTime, GameObject dishPrefab, float eatingDuration)
    {
        DishName = dishName;// Set the dish name
        PreparationTime = preparationTime;// Set the time it will take to prepare the dish
        DishPrefab = dishPrefab;
        EatingDuration = eatingDuration;  // how long it will take the customer to eat the dish
    }

    //create a Salad order
    public static Order CreateSaladOrder(GameObject saladPrefab)
    {
        return new Order("Salad", 5f, saladPrefab, 6f);   // A salad takes 5 seconds to prepare and 6 seconds to eat
    }

    //create a Burger order
    public static Order CreateBurgerOrder(GameObject burgerPrefab)
    {
        return new Order("Burger", 10f, burgerPrefab, 7f);   // A burger takes 10 seconds to prepare and 7 seconds to eat
    }

    //create a Pizza order
    public static Order CreatePizzaOrder(GameObject pizzaPrefab)
    {
        return new Order("Pizza", 15f, pizzaPrefab, 8f);  // // A pizza takes 15 seconds to prepare and 8 seconds to eat
    }
}

