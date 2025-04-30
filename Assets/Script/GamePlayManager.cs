using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    // List of restaurant controllers in the game
    public List<RestaurantController> restaurants;

    // List of sprites representing food items prepared by cooks
    public List<Sprite> spriteCooks;

    public static GamePlayManager instance;

    // Prefab for spawning new customers
    public GameObject goPreCustomer;
    public Transform CusSpawm;

    // Time range for random customer spawn
    public Vector2 timeSpawn;

    // Customer movement speed, patience, and coin
    public float speed;
    public float patience;
    public float coin;

    // Game timer variables
    public float timeGame = 30;
    float _timeGame;

    // UI text element to display remaining game time
    public TextMeshProUGUI txtTime;
   
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        // Initialize the game time
        _timeGame = timeGame;

        // Start the customer spawning coroutine
        StartCoroutine(SpawnCustomer());
    }

    // Coroutine to spawn customers at random intervals based on `timeSpawn`
    IEnumerator SpawnCustomer()
    {
        // Continue spawning customers while the game is still running
        while (_timeGame > 0)
        {
            // Wait for a random period between `timeSpawn.x` and `timeSpawn.y` before spawning the next customer
            yield return new WaitForSeconds(Random.RandomRange(timeSpawn.x, timeSpawn.y));

            // Spawn a new customer
            Spawn();
        }
    }
    private void Update()
    {
        // Only update the game time if it's greater than 0
        if (_timeGame > 0)
        {
            // Decrease the remaining game time
            _timeGame -= Time.deltaTime;

            // Update the UI text to display the remaining time
            txtTime.text = (int)_timeGame + "s";

            // If the game time is over, check the results
            if (_timeGame <= 0)
            {
                // If both restaurants have the same coin balance, reset the game for another round
                if (restaurants[0].coin == restaurants[1].coin)
                {
                    _timeGame = 10;
                    timeGame = 10;
                    StartCoroutine(SpawnCustomer());
                    return;
                }

                // Display the winner restaurant and stop the game
                txtTime.text = FindWin();

                // Pause the game
                Time.timeScale = 0;
            }
        }
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }*/
    }

    // Determines the winner based on which restaurant has more coins
    string FindWin()
    {
        // Return the name of the restaurant with the higher coin balance
        return (restaurants[0].coin > restaurants[1].coin ? restaurants[0].nameRestau : restaurants[1].nameRestau) + " is Win";
    }

    // Spawns a new customer and initializes them with random attributes
    private void Spawn()
    {
        // Instantiate a new customer at the spawn position
        GameObject go = Instantiate(goPreCustomer, CusSpawm.position, Quaternion.identity);

        // Get the CustomerController component from the spawned customer
        CustomerController customerController = go.GetComponent<CustomerController>();

        // Initialize the customer with random speed, patience, and coin values within specified ranges
        customerController.Init(speed * Random.RandomRange(0.8f, 1.2f), patience * Random.RandomRange(0.4f, 3.2f), coin * Random.RandomRange(0.5f, 2));
    }
}
