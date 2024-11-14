using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Order : MonoBehaviour
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }
    public GameObject DishPrefab { get; private set; }

    public Order(string dishName, float price, GameObject dishPrefab)
    {
        DishName = dishName;
        DishPrefab = dishPrefab;
    }

    public float GetPreparationTime() => PreparationTime; // Returns preparation time for the dish

    // Static methods to create orders for different dishes
    public static Order CreateSaladOrder(GameObject saladPrefab)
    {
        return new Order("Salad", 5f, saladPrefab);  // Salad takes 5 seconds to prepare
    }

    public static Order CreateBurgerOrder(GameObject burgerPrefab)
    {
        return new Order("Burger", 10f, burgerPrefab);  // Burger takes 10 seconds to prepare
    }

    public static Order CreatePizzaOrder(GameObject pizzaPrefab)
    {
        return new Order("Pizza", 15f, pizzaPrefab);  // Pizza takes 15 seconds to prepare
    }
}
