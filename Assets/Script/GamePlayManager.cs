using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    public List<RestaurantController> restaurants;
    public List<Sprite> spriteCooks;

    public static GamePlayManager instance;

    public GameObject goPreCustomer;
    public Transform CusSpawm;
    public Vector2 timeSpawn;

    public float speed;
    public float patience;
    public float coin;

    public float timeGame = 30;
    float _timeGame;
    public TextMeshProUGUI txtTime;
    //public Image imgSlider;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        _timeGame = timeGame;
        StartCoroutine(SpawnCustomer());
    }
    IEnumerator SpawnCustomer()
    {
        while (_timeGame > 0)
        {
            yield return new WaitForSeconds(Random.RandomRange(timeSpawn.x, timeSpawn.y));
            Spawn();
        }
    }
    private void Update()
    {
        if (_timeGame > 0)
        {
            _timeGame -= Time.deltaTime;
            txtTime.text = (int)_timeGame + "s";
            //imgSlider.fillAmount = _timeGame / timeGame;
            if (_timeGame <= 0)
            {
                if (restaurants[0].coin == restaurants[1].coin)
                {
                    _timeGame = 10;
                    timeGame = 10;
                    StartCoroutine(SpawnCustomer());
                    return;
                }
                txtTime.text = FindWin();
                Time.timeScale = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn();
        }
    }
    string FindWin()
    {
        return (restaurants[0].coin > restaurants[1].coin ? restaurants[0].nameRestau : restaurants[1].nameRestau) + " is Win";
    }
    private void Spawn()
    {
        GameObject go = Instantiate(goPreCustomer, CusSpawm.position, Quaternion.identity);
        CustomerController customerController = go.GetComponent<CustomerController>();
        customerController.Init(speed * Random.RandomRange(0.8f, 1.2f), patience * Random.RandomRange(0.4f, 3.2f), coin * Random.RandomRange(0.5f, 2));
    }
}
