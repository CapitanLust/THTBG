using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour {

    public Item item;

    //public Button btn_Action;
    public RawImage image;

    public Text tx_Price;
    public Text tx_Name;

    private Shop shop;
    
    public void InitShopItem(Item item, Shop shop)
    {
        this.shop = shop;
        this.item = item;

        tx_Name.text = item.Name;
        tx_Price.text = item.Price + "$";

        //SetButtonAction();

        var newTrigger = new EventTrigger.Entry() { eventID = EventTriggerType.Select };
        newTrigger.callback.AddListener(e => OnSelect(e));
        GetComponent<EventTrigger>().triggers.Add(newTrigger);
    }

    //public void SetButtonAction()
    //{
    //    btn_Action.onClick.RemoveAllListeners();

    //    if (item.IsBought)
    //    {
    //        btn_Action.onClick.AddListener(HandleClickUse);
    //        btn_Action.GetComponentInChildren<Text>().text = "Equip";
    //    }
    //    else
    //    {
    //        btn_Action.onClick.AddListener(HandleClickBuy);
    //        btn_Action.GetComponentInChildren<Text>().text = "Buy";
    //    }
    //}

    public void HandleClickBuy()
    {
        shop.TryToBuyItem(this);
    }

    public void HandleClickUse()
    {
        shop.SetItemToLoadout(this);
    }

    void OnSelect(BaseEventData ed)
    {
        shop.panelInfo.Fill(this);
    }
}
