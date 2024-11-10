using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }
    public GameObject DishPrefab { get; private set; } // Store prefab instead of sprite

    // Constructor to initialize the order with dish name, preparation time, and sprite
    public Order(string dishName, float preparationTime, GameObject dishPrefab)
    {
        DishName = dishName;
        PreparationTime = preparationTime;
        DishPrefab = dishPrefab;
    }

    // This method will return the preparation time for the order
    public float GetPreparationTime() => PreparationTime;
}
