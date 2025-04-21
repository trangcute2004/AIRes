using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STATE_TABLE
{
    Empty,         // Bàn tr?ng
    Reserved,      // Bàn ?ã ???c ??t, khách ?ang ?i t?i
    Ordered,       // Khách ?ã g?i món
    Served         // ?ã ???c ??u b?p nh?n món, ?ang ch? mang ra
}

public class TableController : MonoBehaviour
{
    public Transform standdingWaitStaff;
    public Transform standingFood;
    public Transform standingCus;

    public STATE_TABLE stateTable { get; private set; }
    public DATA_MENU food { get; private set; }
    RestaurantController restaurantController;

    CustomerController customer;

    private void Start()
    {
        stateTable = STATE_TABLE.Empty;
    }
    public void Init(RestaurantController restaurantController)
    {
        this.restaurantController = restaurantController;
    }
    public void ReserveTable(CustomerController customer)
    {
        this.customer = customer;
        stateTable = STATE_TABLE.Reserved;
    }
    public void CheckStateTable()
    {
        if (customer == null)
            stateTable = STATE_TABLE.Empty;
    }

    public void Oder(DATA_MENU food)
    {
        this.food = food;
        stateTable = STATE_TABLE.Ordered;
    }

    public void MarkAsServed()
    {
        customer.SetState(CustomerController.STATE_CUSTOMER.EatFood);
        stateTable = STATE_TABLE.Served;
        standingFood.gameObject.SetActive(true);
        standingFood.GetComponent<SpriteRenderer>().sprite = GamePlayManager.instance.spriteCooks[(int)food];
    }

    public void ClearTable()
    {
        food = default;
        standingFood.gameObject.SetActive(false);
        stateTable = STATE_TABLE.Empty;
    }
}
