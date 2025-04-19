using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CustomerController : MonoBehaviour
{
    float speed;
    public float patience { get; private set; } // s? kiên nh?n, ng??i có s? kiên nh?n cao thì có th? x?p hàng ??i
    float _patience1;
    float _patience2;
    [SerializeField] Image imgShow;
    float coin; // s? ti?n có
    Transform transTarget;
    int indexRestaurant;
    public TableController tableController { get; private set; }
    float timeEat;
    float _timeEat;
    SpriteRenderer _spriteRenderer;
    public enum STATE_CUSTOMER
    {
        ChooseARestaurant, GoToTheRestaurant, GoToTheQueue, GoToTheTable, WaitForFood, EatFood, LeaveTheRestaurant, Default
    }

    public STATE_CUSTOMER state { get; private set; }
    public string State => state.ToString();

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Init(float speed, float patience, float coin)
    {
        this.speed = speed;
        this.patience = patience;
        _patience1 = patience;
        _patience2 = patience;
        this.coin = coin;
        timeEat = patience * Random.RandomRange(0.1f, 0.3f);
        _timeEat = timeEat;
        ChooseRestaurant();
    }

    private async void ChooseRestaurant()
    {
        SetState(STATE_CUSTOMER.ChooseARestaurant);
        int nCheck = 0;
        while (true)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < GamePlayManager.instance.restaurants.Count; i++)
            {
                if (patience >= GamePlayManager.instance.restaurants[i].GetLengthQueue()
                    && GamePlayManager.instance.restaurants[i].CanBuyItem(coin))// ?? ti?n
                {
                    result.Add(i);
                }
            }
            if (result.Count > 0)
            {
                indexRestaurant = Random.RandomRange(0, result.Count);
                SetState(STATE_CUSTOMER.GoToTheRestaurant);
                break;
            }
            nCheck++;
            if (nCheck == 10)
            {
                Debug.Log(gameObject.name + " Khong tim duoc nha hang nao hop y");
                Destroy(gameObject);
            }
            await Task.Delay(500);
        }
    }

    // Update is called once per frame
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
                    _patience1 -= Time.deltaTime;
                    imgShow.fillAmount = _patience1 / _patience2;
                    if (_patience1 <= 0)
                    {
                        //Debug.Log(" Roi di do cho o hang cho qua lau");
                        GamePlayManager.instance.restaurants[indexRestaurant].LeaveQueue(this);
                        Destroy(gameObject);
                    }
                    break;
                }
            case STATE_CUSTOMER.GoToTheTable:
                {
                    if (MoveTarget(tableController.transIndexHuman.position))
                    {
                        List<ItemMenu> foods = GamePlayManager.instance.restaurants[indexRestaurant].ItemCanBuy(coin);
                        int index = Random.RandomRange(0, foods.Count);
                        GamePlayManager.instance.restaurants[indexRestaurant].Oder(this, foods[index].typeFood);
                        //Debug.Log(gameObject.name + " ??t món: " + foods[index].typeItem.ToString());
                        SetState(STATE_CUSTOMER.WaitForFood);
                        _patience1 += patience * 0.5f + foods[index].timeToMakeFood * 1.2f;
                        _patience2 = _patience1;
                    }
                    break;
                }
            case STATE_CUSTOMER.WaitForFood:
                {
                    _patience1 -= Time.deltaTime;
                    imgShow.fillAmount = _patience1 / _patience2;
                    if (_patience1 <= 0)
                    {
                        //Debug.Log(" Roi di do cho do an qua lau");
                        GamePlayManager.instance.restaurants[indexRestaurant].Vote(this, false);
                        Destroy(gameObject);
                    }
                    break;
                }
            case STATE_CUSTOMER.EatFood:
                {
                    _timeEat -= Time.deltaTime;
                    imgShow.fillAmount = _timeEat / timeEat;
                    //Debug.Log(_timeEat / timeEat);
                    if (_timeEat <= 0)
                    {
                        //Debug.Log(transform.name + " thanh toan");
                        GamePlayManager.instance.restaurants[indexRestaurant].Pay(this, tableController);
                        SetState(STATE_CUSTOMER.LeaveTheRestaurant);
                    }
                    break;
                }
            case STATE_CUSTOMER.LeaveTheRestaurant:
                {
                    if (MoveTarget(GamePlayManager.instance.restaurants[indexRestaurant].door.transform.position))
                    {
                        GamePlayManager.instance.restaurants[indexRestaurant].door.Open();
                        Destroy(gameObject);
                    }

                    break;
                }
        }
    }
    bool MoveTarget(Vector2 pTarget)
    {
        transform.position = Vector2.Lerp(transform.position, pTarget, speed * Time.deltaTime);
        _spriteRenderer.flipX = transform.position.x >= pTarget.x;
        if (Vector2.Distance(transform.position, pTarget) < 0.1f) return true;
        return false;
    }
    private void UpdateStateGoToTheRestaurant()
    {
        if (MoveTarget(GamePlayManager.instance.restaurants[indexRestaurant].door.transform.position))
        {
            GamePlayManager.instance.restaurants[indexRestaurant].door.Open();
            tableController = GamePlayManager.instance.restaurants[indexRestaurant].GetTableFree();
            if (GamePlayManager.instance.restaurants[indexRestaurant].GetLengthQueue() > 0 || tableController == null)
            {
                //Debug.Log(gameObject.name + " vào hàng ch? do quá ?ông ");
                SetState(STATE_CUSTOMER.GoToTheQueue);
                GamePlayManager.instance.restaurants[indexRestaurant].AddQueue(this);
            }
            else
            {
                //Debug.Log(gameObject.name + " vào bàn ?n");
                tableController.ReserveTable(this);
                SetState(STATE_CUSTOMER.GoToTheTable);
            }
        }
    }
    public void OutQueue(TableController tableController)
    {
        this.tableController = tableController;
        tableController.ReserveTable(this);
        //Debug.Log(gameObject.name + " r?i kh?i hàng ??i t?i bàn ?n");
        SetState(STATE_CUSTOMER.GoToTheTable);
    }
    public void SetState(STATE_CUSTOMER s)
    {
        state = s;
        if (state == STATE_CUSTOMER.GoToTheQueue || state == STATE_CUSTOMER.WaitForFood || state == STATE_CUSTOMER.EatFood)
            imgShow.gameObject.SetActive(true);
        else imgShow.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        Destroy(gameObject, 0.5f);
    }
}
