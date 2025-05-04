using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class CustomerController : MonoBehaviour
{
    //speed of customer
    float speed;
    // Customer's patience (how long they will wait before leaving)
    public float patience { get; private set; } 

    // Temporary patience values for tracking
    float _patience1;
    float _patience2;

    //countdown patiende of cus
    [SerializeField] Image imgShow;

    // money of cus
    float coin;

    // Target position for movement
    Transform transTarget;

    // Index of the restaurant the customer chooses
    int indexRestaurant;

    // The table that the customer is sitting at
    public TableController tableController { get; private set; }

    // Time taken to eat
    float timeEat;
    float _timeEat;

    SpriteRenderer _spriteRenderer;

    //FINITE STATE MACHINE
    public enum STATE_CUSTOMER {ChooseARestaurant, GoToTheRestaurant, GoToTheQueue, GoToTheTable, WaitForFood, EatFood, LeaveTheRestaurant}

    // store current state of the customer
    public STATE_CUSTOMER state { get; private set; }
    // Returns (retrieve) the current state as a string (debug)
    public string State => state.ToString();

    void Start()
    {
        // Get the sprite renderer attached to the customer
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Initializes the customer with speed, patience, and coin values
    public void Init(float speed, float patience, float coin)
    {
        this.speed = speed;
        this.patience = patience;
        _patience1 = patience; // Set initial patience
        _patience2 = patience; // Set initial patience for resetting
        this.coin = coin;

        // Calculate time to eat based on patience and a random factor
        timeEat = patience * Random.RandomRange(0.1f, 0.3f);
        _timeEat = timeEat;

        // Start the process of choosing a restaurant
        ChooseRestaurant();
    }

    // Handles the process of selecting a restaurant based on patience and coin
    private async void ChooseRestaurant()
    {
        //Set initial state to choosing a restaurant
        SetState(STATE_CUSTOMER.ChooseARestaurant);
        int nCheck = 0; //Counter to limit the number of attempts

        //loop to keep trying until a suitable restaurant is found
        while (true)
        {
            //Stores indices of suitable restaurants
            List<int> result = new List<int>();

            // Check each restaurant in the list
            for (int i = 0; i < GamePlayManager.instance.restaurants.Count; i++)
            {
                // Only add restaurant if customer can afford it and has enough patience to wait for the length queue, has enough coin to pay
                if (patience >= GamePlayManager.instance.restaurants[i].GetLengthQueue()
                    && GamePlayManager.instance.restaurants[i].CanBuyItem(coin))
                {
                    result.Add(i); // Add the index of the suitable restaurant
                }
            }

            // If a suitable restaurant is found, pick one randomly and go there
            if (result.Count > 0)
            {
                indexRestaurant = Random.RandomRange(0, result.Count); // Randomly select a restaurant
                SetState(STATE_CUSTOMER.GoToTheRestaurant); // Set state to going to the restaurant
                break;
            }

            // Increment the attempt counter
            nCheck++;

            // If 10 attempts have been made without finding a suitable restaurant, destroy the customer
            if (nCheck == 10)
            {
                Debug.Log(gameObject.name + "cant find restaurant");
                Destroy(gameObject);
            }

            // Wait for half a second before retrying
            await Task.Delay(500);
        }
    }

    void Update()
    {
        switch (state)
        {
            case STATE_CUSTOMER.GoToTheRestaurant:
                {
                    UpdateStateGoToTheRestaurant();
                    break;
                }
            case STATE_CUSTOMER.GoToTheQueue:
                {
                    // Decrease the customer's patience over time while they wait
                    _patience1 -= Time.deltaTime;

                    // Update the patience image
                    imgShow.fillAmount = _patience1 / _patience2;

                    // If the customer's patience runs out, they leave the queue and the customer object is destroyed
                    if (_patience1 <= 0)
                    {
                        // Remove the customer from the queue
                        GamePlayManager.instance.restaurants[indexRestaurant].LeaveQueue(this);

                        // Destroy the customer game object since they left the queue
                        Destroy(gameObject);
                    }
                    break;
                }
            case STATE_CUSTOMER.GoToTheTable:
                {
                    // If the customer reaches the table, proceed with ordering food
                    if (MoveTarget(tableController.standingCus.position))
                    {
                        // Get the list of foods that the customer can afford
                        List<ItemMenu> foods = GamePlayManager.instance.restaurants[indexRestaurant].ItemCanBuy(coin);

                        // Randomly select a food item from the list
                        int index = Random.RandomRange(0, foods.Count);

                        // Place the order for the selected food
                        GamePlayManager.instance.restaurants[indexRestaurant].Oder(this, foods[index].typeFood);
                        
                        SetState(STATE_CUSTOMER.WaitForFood);

                        // Adjust patience based on the time to make the food and some additional factor
                        _patience1 += patience * 0.5f + foods[index].timeToMakeFood * 1.2f;
                        _patience2 = _patience1; // Reset the patience maximum value
                    }
                    break;
                }
            case STATE_CUSTOMER.WaitForFood:
                {
                    // Decrease patience over time while waiting for food
                    _patience1 -= Time.deltaTime;

                    // Update the patience progress image
                    imgShow.fillAmount = _patience1 / _patience2;

                    // If the customer's patience runs out, they leave the restaurant without eating
                    if (_patience1 <= 0)
                    {
                        // Call the restaurant to show the customer is not satisfied
                        GamePlayManager.instance.restaurants[indexRestaurant].Vote(this, false);
                        // Destroy the customer game object
                        Destroy(gameObject);
                    }
                    break;
                }
            case STATE_CUSTOMER.EatFood:
                {
                    // Decrease the time the customer is eating
                    _timeEat -= Time.deltaTime;

                    // Update the eating progress image
                    imgShow.fillAmount = _timeEat / timeEat;

                    // If the customer finishes eating, they proceed to pay and leave
                    if (_timeEat <= 0)
                    {
                        // Call the restaurant to process the payment
                        GamePlayManager.instance.restaurants[indexRestaurant].Pay(this, tableController);

                        SetState(STATE_CUSTOMER.LeaveTheRestaurant);
                    }
                    break;
                }
            case STATE_CUSTOMER.LeaveTheRestaurant:
                {
                    // If the customer reaches the restaurant door, open it and destroy the customer
                    if (MoveTarget(GamePlayManager.instance.restaurants[indexRestaurant].door.transform.position))
                    {
                        // Open the door of the restaurant as the customer leaves
                        GamePlayManager.instance.restaurants[indexRestaurant].door.Open();
                        // Destroy the customer game object after they leave
                        Destroy(gameObject);
                    }

                    break;
                }
        }
    }

    // Move towards a target position
    bool MoveTarget(Vector2 pTarget)
    {
        // Move smoothly towards the target position using Lerp (linear interpolation)
        transform.position = Vector2.Lerp(transform.position, pTarget, speed * Time.deltaTime);
        // Return true if the customer has reached the target position
        if (Vector2.Distance(transform.position, pTarget) < 0.1f) return true;
        return false;
    }

    // Handle the customer's movement to the restaurant
    private void UpdateStateGoToTheRestaurant()
    {
        if (MoveTarget(GamePlayManager.instance.restaurants[indexRestaurant].door.transform.position))
        {
            // If the customer reaches the restaurant door, open it
            GamePlayManager.instance.restaurants[indexRestaurant].door.Open();

            // Get a free table for the customer
            tableController = GamePlayManager.instance.restaurants[indexRestaurant].GetTableFree();

            // If there are people in the queue or no free table, go to the queue
            if (GamePlayManager.instance.restaurants[indexRestaurant].GetLengthQueue() > 0 || tableController == null)
            {
                SetState(STATE_CUSTOMER.GoToTheQueue);
                GamePlayManager.instance.restaurants[indexRestaurant].AddQueue(this);
            }
            else
            {
                // Otherwise, reserve the table and move to the table
                tableController.ReserveTable(this);
                SetState(STATE_CUSTOMER.GoToTheTable);
            }
        }
    }

    //when the customer leaves the queue and is assigned to a table.
    public void OutQueue(TableController tableController)
    {
        // Set the table that the customer is assigned to
        this.tableController = tableController;
        // Reserve the table for the customer.
        tableController.ReserveTable(this);
        
        SetState(STATE_CUSTOMER.GoToTheTable);
    }

    //setting the customer's current state.
    public void SetState(STATE_CUSTOMER s)
    {
        // Update the current state
        state = s;

        // If the state is one of the following, show the patience progress bar (imgShow):
        // - GoToTheQueue: Customer is in the queue.
        // - WaitForFood: Customer is waiting for food.
        // - EatFood: Customer is eating food.
        if (state == STATE_CUSTOMER.GoToTheQueue || state == STATE_CUSTOMER.WaitForFood || state == STATE_CUSTOMER.EatFood)
            imgShow.gameObject.SetActive(true); // Show the patience image.
        else imgShow.gameObject.SetActive(false); // Hide the patience image.
    }

    // when the customer object is being destroyed
    private void OnDestroy()
    {
        // Destroy the customer game object after a short delay 0.5 seconds
        Destroy(gameObject, 0.5f);
    }
}
