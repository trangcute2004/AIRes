using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Order : MonoBehaviour
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }
    public GameObject DishPrefab { get; set; } // Change this to 'public' or 'protected'

    public Order(string dishName, float preparationTime, GameObject dishPrefab)
    {
        DishName = dishName;
        PreparationTime = preparationTime;
        DishPrefab = dishPrefab;
    }

    public float GetPreparationTime() => PreparationTime;

    public static Order CreateSaladOrder(GameObject saladPrefab)
    {
        return new Order("Salad", 5f, saladPrefab);
    }

    public static Order CreateBurgerOrder(GameObject burgerPrefab)
    {
        return new Order("Burger", 10f, burgerPrefab);
    }

    public static Order CreatePizzaOrder(GameObject pizzaPrefab)
    {
        return new Order("Pizza", 15f, pizzaPrefab);
    }
}
