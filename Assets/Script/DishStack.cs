using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishStack : MonoBehaviour
{
    public GameObject saladPrefab;  
    public GameObject burgerPrefab; 
    public GameObject pizzaPrefab;  

    public int maxStackSize = 5;    // Maximum number of dishes in the stack
    public Vector2 stackStartPosition; // Starting position of the stack in scene
    public float stackSpacing = 0.5f;  // Vertical spacing between dishes in the stack

    //store the dishes in the stack.
    private List<GameObject> dishStack = new List<GameObject>();

    void Start()
    {
        // Initialize the stack with some dishes
        AddDishToStack(saladPrefab);
        AddDishToStack(burgerPrefab);
        AddDishToStack(pizzaPrefab);
    }

    // add a dish to the stack.
    public void AddDishToStack(GameObject dishPrefab)
    {
        // Check if the stack has reached its maximum size.
        if (dishStack.Count >= maxStackSize)
        {
            Debug.LogWarning("Dish stack is full. Cannot add more dishes.");
            return;
        }

        // Calculate the position where the new dish should be placed.
        Vector3 dishPosition = new Vector3(stackStartPosition.x, stackStartPosition.y - (dishStack.Count * stackSpacing), 0);

        // Instantiate the new dish at the correct position
        GameObject newDish = Instantiate(dishPrefab, dishPosition, Quaternion.identity, transform);

        // Add the dish to the stack
        dishStack.Add(newDish);
    }

    // Removes the top dish from the stack
    public GameObject RemoveDishFromStack()
    {
        //Check if the dish stack is empty
        if (dishStack.Count == 0)
        {
            Debug.LogWarning("Dish stack is empty. No dishes to remove.");
            return null;
        }

        // Get the top dish
        GameObject topDish = dishStack[dishStack.Count - 1];
        dishStack.RemoveAt(dishStack.Count - 1);

        // Destroy or return the top dish
        return topDish;
    }

    // Clears all dishes from the stack
    public void ClearStack()
    {
        foreach (var dish in dishStack)
        {
            Destroy(dish);
        }
        dishStack.Clear();
    }
}