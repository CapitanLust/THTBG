using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

//change to struct?
[System.Serializable]
public class Item
{
    public Image Image;

    public int ID;
    public string Name;
    public string Description;
    public string Type;

    public int Price;
    public int Level;

    public bool IsBought;
}
public class LoadoutInfo
{
    public string currentLoadoutName;
    public string lastLoadoutName;

    public Item weapon;
    public Item gadget;
    public Item skin;
    public Item skill;

    //use for check changes
    //(false - need sync with DB)
    public bool isSynchronized;
    
    //old
    //public string weaponName;
    //public string gadgetName;
    //public string skinName;
    //public string skillName;
}

//change class name
public class Shop : MonoBehaviour
{
    [Serializable]
    public class PanelInfo
    {
        public Text tx_name, tx_description,
                    tx_PriceLvl;
        public Button btn_Action;
        public Shop shop;

        //ShopItem and not just Item -- for link action callback in future
        public void Fill(ShopItem shopItem)
        {
            tx_name.text = shopItem.item.Name;
            tx_description.text = shopItem.item.Description;
            tx_PriceLvl.text = "<Color=#FBF16BFF>"+shopItem.item.Price
                             + "$</Color>  <Color=#9CF5F5FF>"
                             + shopItem.item.Level+"lvl</Color>";
            SetButtonAction(shopItem);
        }

        public void SetButtonAction(ShopItem shopItem)
        {
            btn_Action.onClick.RemoveAllListeners();
            btn_Action.interactable = true;

            if (shopItem.item.IsBought)
            {
                LoadoutInfo loadoutInfo = shop.loadoutsList[shop.dropdownLoadouts.value];
                string loadoutItemName = "";

                switch (shopItem.item.Type)
                {
                    case "weapon": loadoutItemName = loadoutInfo.weapon.Name; break;
                    case "gadget": loadoutItemName = loadoutInfo.gadget.Name; break;
                    case "skill": loadoutItemName = loadoutInfo.skill.Name; break;
                    case "skin": loadoutItemName = loadoutInfo.skin.Name; break;
                }

                if (shopItem.item.Name == loadoutItemName)
                {
                    btn_Action.interactable = false;
                    btn_Action.GetComponentInChildren<Text>().text = "Equiped";
                }
                else
                {
                    btn_Action.onClick.AddListener(()=> {
                        shop.SetItemToLoadout(shopItem); });
                    btn_Action.GetComponentInChildren<Text>().text = "Equip";
                }
            }
            else
            {
                btn_Action.onClick.AddListener(()=> {
                    shop.StartCoroutine(shop.TryToBuyItem(shopItem)); });
                btn_Action.GetComponentInChildren<Text>().text = "Buy";
            }
        }
    }

    private int money = 35, lvl = 9; // 35, 9 -- just for mockup in debug
    public ShopItem itemPrefab;

    public Transform contentPanel;

    public Text tx_PlayerMoney;
    public Text tx_PlayerLevel;

    public Slider slider_Filter;

    public PanelInfo panelInfo;

    //dropdown loadouts
    public Dropdown dropdownLoadouts;
    public Button btn_loadoutSettingsMenu;
    //settings panel
    public GameObject loadoutSettingMenu;
    public Button btn_loadoutRename;
    public Button btn_loadoutRemove;
    public Button btn_loadoutCreate;
    public Button btn_loadoutSave;
    //Input panel
    public GameObject inputLoadoutName;
    public InputField input_loadoutName;
    public Button btn_confirm;
    public Button btn_cancel;
    //to lock LoadoutSettingMenu
    bool     isLoadoutChanging; 

    public int Money
    {
        get { return money; }
        set
        {
            money = value;
            tx_PlayerMoney.text = money + "$";
        }
    }
    public int Level
    {
        get { return lvl; }
        set
        {
            lvl = value;
            tx_PlayerLevel.text = lvl + "lvl";
        }
    }

    public List<LoadoutInfo> loadoutsList = new List<LoadoutInfo>();

    public List<Item> allItemList = new List<Item>();
    public List<Item> showingItemList;

    // Use this for initialization
    IEnumerator Start ()
    {
        yield return StartCoroutine(DataBaseConnector.SelectPlayerInfo());
        yield return StartCoroutine(DataBaseConnector.SelectItems(this));
        yield return StartCoroutine(DataBaseConnector.SelectPurchasedItems(this));
        yield return StartCoroutine(DataBaseConnector.SelectLoadouts(this));

        showingItemList = allItemList;

        UpdatePlayerInfo();
        RefreshShop();
        RefreshLoadouts(0);

        btn_loadoutRename.onClick.AddListener(OpenLoadoutRename);
        btn_loadoutSave.onClick.AddListener(OnLoadoutSynchronize);
    }

    public IEnumerator TryToBuyItem(ShopItem s)
    {
        if (Money >= s.item.Price && Level >= s.item.Level)
        {
            //SQL transaction
            Debug.Log("start");
            yield return StartCoroutine(DataBaseConnector.BuyItem(s.item));

            s.item.IsBought = true;
            panelInfo.SetButtonAction(s);

            Money -= s.item.Price;
            //
        }
        else
            Debug.Log("not enough money or level");
    }

    public void SetItemToLoadout(ShopItem s)
    {
        LoadoutInfo loadoutInfo = loadoutsList[dropdownLoadouts.value];

        switch (s.item.Type)
        {
            case "weapon": loadoutInfo.weapon = s.item; break;
            case "gadget": loadoutInfo.gadget = s.item; break;
            case "skill": loadoutInfo.skill = s.item; break;
            case "skin": loadoutInfo.skin = s.item; break;
        }

        panelInfo.SetButtonAction(s);
        loadoutInfo.isSynchronized = false;
    }

    public void CloseInputLoadoutName()
    {
        input_loadoutName.text = "";
        inputLoadoutName.SetActive(false);
    }

    public void OpenLoadoutRename()
    {
        //isLoadoutChanging = true;
        inputLoadoutName.SetActive(true);
        //loadoutSettingMenu.SetActive(false);

        btn_confirm.onClick.RemoveAllListeners();
        btn_confirm.onClick.AddListener(OnConfirmLoadoutRename);

        btn_cancel.onClick.RemoveAllListeners();
        btn_cancel.onClick.AddListener(CloseInputLoadoutName);
    }

    public void OnConfirmLoadoutRename()
    {
        LoadoutInfo loadoutInfo = loadoutsList[dropdownLoadouts.value];
        string newLoadoutName = input_loadoutName.text;

        //here will be validation block or function
        if (newLoadoutName == "")
        {
            Debug.Log("empty name");
            return;
        }

        if (newLoadoutName == loadoutInfo.currentLoadoutName)
        {
            Debug.Log("the same name");
            return;
        }

        foreach (var loadout in loadoutsList)
        {
            if (loadout.currentLoadoutName == newLoadoutName)
            {
                Debug.Log("this name is already exist");
                return;
            }
        }

        loadoutInfo.currentLoadoutName = newLoadoutName;
        loadoutInfo.isSynchronized = false;
        RefreshLoadouts(dropdownLoadouts.value);
        CloseInputLoadoutName();
    }

    public void OnLoadoutSynchronize()
    {
        foreach (var loadout in loadoutsList)
        {
            if (!loadout.isSynchronized)
            {
                StartCoroutine(DataBaseConnector.SynchronizeLoadouts(loadout));
                loadout.lastLoadoutName = loadout.currentLoadoutName;
                loadout.isSynchronized = true;
            }
        }
    }

    public void isEquipped()
    {

    }
     
    private void RefreshShop()
    {
        foreach (Transform t in contentPanel)
            Destroy(t.gameObject);

        foreach (var item in showingItemList)
            AddItem(item);
    }

    private void RefreshLoadouts(int index)
    {
        dropdownLoadouts.ClearOptions();

        foreach (var loadout in loadoutsList)
            dropdownLoadouts.options.Add(new Dropdown.OptionData
            {
                text = loadout.currentLoadoutName
            });

        //???
        dropdownLoadouts.GetComponentInChildren<Text>().text = dropdownLoadouts.options[index].text;
    }

    public void FilterItemList()
    {
        var filteringType = FilterSliderValue2ItemType();
        if (filteringType == "all")
            showingItemList = allItemList;
        else
            showingItemList = allItemList.FindAll(x => x.Type == filteringType);

        RefreshShop();
    }

    public void AddItem (Item item)
    {
        Instantiate(itemPrefab, contentPanel)
            .InitShopItem(item, this);
    }
    
    private string FilterSliderValue2ItemType()
    {
        switch (Convert.ToInt32(slider_Filter.value))
        {
            case 0: return "weapon";
            case 1: return "gadget";
            case 2: return "skill";
            case 3: return "skin";
            case 4: return "all";
        }
        return "";
    }

    public void UpdatePlayerInfo()
    {
        Money = Cmn.Money;

        //TODO convert xp to lvl
        Level = Cmn.XP / 100;
    }
}