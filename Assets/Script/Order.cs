using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order: MonoBehaviour
{
    public string DishName { get; private set; }
    public float PreparationTime { get; private set; }

    public Order(string dishName, float preparationTime)
    {
        DishName = dishName;
        PreparationTime = preparationTime;
    }

    public float GetPreparationTime() => PreparationTime;
}

