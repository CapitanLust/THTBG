using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour {

    public Item currentItem;

    public Button actionButton;
    public Image image;

    public Text textDescription;
    public Text textPrice;
    public Text textLevel;
    public Text textName;

    private string type;
    private bool isBought;

    private Shop shop;
    // Use this for initialization
    void Start()
    {
        //empty?
    }

    public void Setup(Item item, Shop shop)
    {
        this.shop = shop;
        currentItem = item;

        textName.text = item.name;
        textDescription.text = item.description;
        textPrice.text = "Price: " + item.price.ToString();
        textLevel.text = "Level: " + item.level.ToString();

        image = item.image;

        type = item.type;
        isBought = item.isBought;

        SetButtonActive();
    }

    private void SetButtonActive()
    {
        actionButton.onClick.RemoveAllListeners();

        if (isBought)
        {
            actionButton.onClick.AddListener(HandleClickBuy);
            actionButton.GetComponentInChildren<Text>().text = "Use";
        }
        else
        {
            actionButton.onClick.AddListener(HandleClickUse);
            actionButton.GetComponentInChildren<Text>().text = "Buy";
        }
    }

    public void HandleClickBuy()
    {
        shop.TryToBuyItem(currentItem);
    }

    public void HandleClickUse()
    {
        shop.SetItemToLoadout(currentItem);
    }
}
