using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Item
{
    public Image image;

    public string name;
    public string description;
    public string type;

    public int price;
    public int level;

    public bool isBought;
}

public class Shop : MonoBehaviour
{
    public GameObject itemPrefab;

    public Transform contentPanel;

    public Text playerMoney;
    public Text playerLevel;

    public int money;
    public int level;

    public List<Item> itemList = new List<Item>();

	// Use this for initialization
	void Start ()
    {
        //get items from DB
        DB_connector.OpenConnection();//TODO replace and use 1 time
        itemList = DB_connector.GetItemsInfo(1);

        AddItems();
	}

    public void TryToBuyItem(Item item)
    {
        if (money >= item.price && level >= item.level)
        {
            //SQL query

            item.isBought = true;

            money -= item.price;
            playerMoney.text = money.ToString();

            //Event or delegate
        }
        else
            Debug.Log("not enough money or level");
    }

    public void SetItemToLoadout(Item item)
    {
        //cast type and set item to loadout
    }

    private void AddItems()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            //info about item
            Item item = itemList[i];

            //add new object into content of scroll view
            GameObject newItem = Instantiate(itemPrefab);
            newItem.transform.SetParent(contentPanel, false);

            ShopItem shopItem = newItem.GetComponent<ShopItem>();
            shopItem.Setup(item, this);
        }
    }
}
