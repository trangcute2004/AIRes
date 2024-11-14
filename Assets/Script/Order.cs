using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }
    public GameObject DishPrefab { get; private set; }

    public Order(string dishName, float preparationTime, GameObject dishPrefab)
    {
        if (string.IsNullOrEmpty(dishName))
        {
            Debug.LogError("Dish name cannot be null or empty.");
            DishName = "Unknown";
        }
        else
        {
            DishName = dishName;
        }

        if (dishPrefab == null)
        {
            Debug.LogError($"Dish prefab is null for {DishName}.");
        }
        else
        {
            DishPrefab = dishPrefab;
        }

        PreparationTime = preparationTime;
        Debug.Log($"Order created successfully: {DishName}, Prefab: {DishPrefab?.name ?? "null"}");
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
