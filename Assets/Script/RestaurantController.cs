using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum DATA_MENU
{
    Salad,
    Burger,
    Pizza,
}
[System.Serializable]
public class ItemMenu // lo?i: giá, th?i gian làm
{
    public DATA_MENU typeFood;
    public int price;
    public float timeToMakeFood;
}
public class RestaurantController : MonoBehaviour
{
    public string nameRestau;

    [SerializeField] List<TableController> tables;

    public Door door;
    [SerializeField] List<ItemMenu> menus;

    [SerializeField] int maxQueuCus = 5;
    List<CustomerController> queueCus;

    [SerializeField] WaitStaffController waitStaff;
    [SerializeField] Transform transWaistaff;

    public List<TableController> queueTables = new List<TableController>();
    public int coin { get; private set; }

    [SerializeField] TextMeshProUGUI txtName;
    [SerializeField] TextMeshProUGUI txtDes;

    // Start is called before the first frame update
    void Start()
    {
        SetCoin(0);
        queueCus = new List<CustomerController>();
        txtName.text = nameRestau;
        waitStaff.Init(transWaistaff, this);
    }
    void UpdateDes()
    {
        txtDes.text = "$" + coin;
    }
    public bool IsFull()
    {
        return GetTableFree() != null;
    }
    public TableController GetTableFree()
    {
        for (int i = 0; i < tables.Count; i++)
            if (tables[i].stateTable == STATE_TABLE.Empty)
                return tables[i];
        return null;
    }
    public int GetLengthQueue() => queueCus.Count;
    public void AddQueue(CustomerController customerController)
    {
        if (queueCus.Count == maxQueuCus)
        {
            Vote(customerController, false);
            Destroy(customerController);
        }
        else
        {
            queueCus.Add(customerController);
        }
    }
    public void LeaveQueue(CustomerController customerController)
    {
        Vote(customerController, false);
        queueCus.Remove(customerController);
    }
    public void Oder(CustomerController customerController, DATA_MENU food)
    {
        customerController.tableController.Oder(food);
        queueTables.Add(customerController.tableController);
    }
    public void Pay(CustomerController customerController, TableController table)
    {
        var item = FindItem(table.food);
        item.timeToMakeFood *= 0.95f;
        SetCoin(coin + item.price);
        Vote(customerController, true);
    }

    public void Vote(CustomerController customerController, bool isAppreciate)
    {
        if (customerController.tableController != null)
        {
            customerController.tableController.ClearTable();
            if (queueCus.Count > 0)
            {
                queueCus[0].OutQueue(customerController.tableController);
                queueCus.RemoveAt(0);
            }
            if (queueTables.Contains(customerController.tableController))
            {
                queueTables.Remove(customerController.tableController);
            }
        }
        if (!isAppreciate)
        {
            SetCoin(coin - 2);
        }
    }
    public bool CanBuyItem(float coin)
    {
        return ItemCanBuy(coin).Count > 0;
    }
    public List<ItemMenu> ItemCanBuy(float coin)
    {
        List<ItemMenu> result = new List<ItemMenu>();
        for (int i = 0; i < menus.Count; i++)
            if (menus[i].price <= coin)
                result.Add(menus[i]);
        return result;
    }
    public ItemMenu FindItem(DATA_MENU food)
    {
        return menus.Find(x => x.typeFood == food);
    }
    void SetCoin(int value)
    {
        coin = Mathf.Clamp(value, 0, 9999);
        UpdateDes();
    }
}
