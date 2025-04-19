using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STATE_WAITSTAFF
{
    Idle,
    Cooking,
    MoveToServe,
    MoveToIdle
}


public class WaitStaffController : MonoBehaviour
{
    public float skillLevel = 3f;
    public float speed = 2f;

    [SerializeField] List<Sprite> sprites = new List<Sprite>();

    private STATE_WAITSTAFF state = STATE_WAITSTAFF.Idle;
    private float cookingTimeLeft = 0f;

    private RestaurantController restaurantController;
    private Transform restPosition; // v? trí transCook c?a nhà hàng
    private SpriteRenderer skin;

    private void Awake()
    {
        skin = GetComponent<SpriteRenderer>();
    }
    public void Init(Transform trans, RestaurantController restaurantController)
    {
        SetState(STATE_WAITSTAFF.Idle);
        restPosition = trans;
        this.restaurantController = restaurantController;
    }

    void Update()
    {
        switch (state)
        {
            case STATE_WAITSTAFF.Idle:
                HandleIdle();
                break;

            case STATE_WAITSTAFF.Cooking:
                HandleCooking();
                break;

            case STATE_WAITSTAFF.MoveToServe:
                if (restaurantController.queueTables.Count == 0)
                {
                    SetState(STATE_WAITSTAFF.MoveToIdle);
                    break;
                }
                MoveTo(restaurantController.queueTables[0].transIndexChef.position, () =>
                {
                    restaurantController.queueTables[0].MarkAsServed();
                    restaurantController.queueTables.RemoveAt(0);
                    SetState(STATE_WAITSTAFF.MoveToIdle);
                });
                break;
            case STATE_WAITSTAFF.MoveToIdle:
                MoveTo(restPosition.position, () => SetState(STATE_WAITSTAFF.Idle));
                break;
        }
    }


    void HandleIdle()
    {
        if (restaurantController.queueTables.Count != 0)
        {
            SetState(STATE_WAITSTAFF.Cooking);
            DATA_MENU food = restaurantController.queueTables[0].food;
            cookingTimeLeft = AdjustCookingTime(restaurantController.FindItem(food).timeToMakeFood);
        }
    }

    void HandleCooking()
    {
        if (cookingTimeLeft <= 0)
        {
            Debug.Log("Chef ?ã n?u xong món");
            skillLevel *= 1.05f;
            SetState(STATE_WAITSTAFF.MoveToServe);
        }
        else
        {
            cookingTimeLeft -= Time.deltaTime;
        }
    }


    void MoveTo(Vector2 target, System.Action onArrive)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            onArrive.Invoke();
        }
    }
    float AdjustCookingTime(float baseTime)
    {
        if (skillLevel >= 3f)
            return baseTime * (1f - 0.05f * (skillLevel - 2f));
        else
            return baseTime * (1f + 0.07f + (2f - skillLevel) * 0.06f);
    }

    CustomerController FindCustomerAtTable(TableController table)
    {
        var customers = FindObjectsOfType<CustomerController>();
        foreach (var c in customers)
        {
            if (Vector2.Distance(c.transform.position, table.transIndexHuman.position) < 0.2f)
                return c;
        }
        return null;
    }
    void SetState(STATE_WAITSTAFF s)
    {
        state = s;
        skin.sprite = sprites[(int)state];
    }
}
