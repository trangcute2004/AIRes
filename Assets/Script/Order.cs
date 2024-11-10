using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }

    // Constructor to initialize the order with dish name and preparation time
    public Order(string dishName, float preparationTime)
    {
        DishName = dishName;
        PreparationTime = preparationTime;
    }

    // This method will return the preparation time for the order
    public float GetPreparationTime() => PreparationTime;
}
