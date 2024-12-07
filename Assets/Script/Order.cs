using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Order
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }
    public GameObject DishPrefab { get; set; }
    public bool IsDelivered = false;
    public float EatingDuration { get; private set; }  // New field for eating duration

    public Order(string dishName, float preparationTime, GameObject dishPrefab, float eatingDuration)
    {
        DishName = dishName;
        PreparationTime = preparationTime;
        DishPrefab = dishPrefab;
        EatingDuration = eatingDuration;  // Initialize with a specific eating time
    }

    public static Order CreateSaladOrder(GameObject saladPrefab)
    {
        return new Order("Salad", 5f, saladPrefab, 5f);  // 3 seconds to eat salad
    }

    public static Order CreateBurgerOrder(GameObject burgerPrefab)
    {
        return new Order("Burger", 10f, burgerPrefab, 6f);  // 5 seconds to eat burger
    }

    public static Order CreatePizzaOrder(GameObject pizzaPrefab)
    {
        return new Order("Pizza", 15f, pizzaPrefab, 8f);  // 8 seconds to eat pizza
    }
}

