using System.Collections;
using System.Security.Cryptography;
using System;
using UnityEngine;

static public class DataBaseConnector
{
    private static string serverPath = "http://exergonic-frequency.000webhostapp.com/";

    private static string registrationURL = serverPath + "registration.php";
    private static string authenticationURL = serverPath + "authentication.php";
    private static string mailExistURL = serverPath + "is mail exist.php";
    private static string loginExistURL = serverPath + "is login exist.php";

    private static string buyItemURL = serverPath + "buy item.php";

    private static string selectItemsURL = serverPath + "select items.php";
    private static string selectPurchasedItemsURL = serverPath + "bought items.php";
    private static string selectStatURL = serverPath + "select statistic.php";
    private static string selectLoadoutsURL = serverPath + "select loadouts.php";
    private static string selectPlayerInfoURL = serverPath + "select player info.php";

    private static string weaponsURL = serverPath + "weapons.php";
    private static string gadgetsURL = "";
    private static string skinsURL = "";
    private static string skillsURL = "";

    private static string synchronizeLoadoutsURL = serverPath + "synchronize loadouts.php";

    /// <summary>
    /// confirm is key word(confirm means reg succesfull)
    /// </summary>
    static public IEnumerator RegistrationPlayer(string login, 
        string mail, string password, Validation v)
    {
        WWWForm form = new WWWForm();

        form.AddField("login", login);
        form.AddField("mail", mail);
        form.AddField("password", password);
        
        WWW registration = new WWW(registrationURL, form);

        yield return registration;

        v.ConfirmSignUp = registration.text.Trim() == "confirm";
    }

    /// <summary>
    /// confirm is key word(confirm means auth succesfull)
    /// </summary>
    static public IEnumerator AuthenticationPlayer(string login, 
        string password, Validation v)
    {
        WWWForm form = new WWWForm();

        form.AddField("login", login);
        form.AddField("password", password);

        WWW authentication = new WWW(authenticationURL, form);

        yield return authentication;

        string[] result = authentication.text.Split('|');
        v.ConfirmSignIn = result[0].Trim() == "confirm";
        v.ID = v.ConfirmSignIn ? int.Parse(result[1]) : -1;
    }

    static public IEnumerator MailExist(string mail, Validation v)
    {
        WWWForm form = new WWWForm();
        form.AddField("mail", mail);
        WWW mailExist = new WWW(mailExistURL, form);

        yield return mailExist;

        //WTF??? does not work without trim() LOL :D
        v.MailExist = (mailExist.text.Trim() == "exist");
    }

    static public IEnumerator LoginExist(string login, Validation v)
    {
        WWWForm form = new WWWForm();
        form.AddField("login", login);
        WWW loginExist = new WWW(loginExistURL, form);

        yield return loginExist;

        //WTF??? does not work without trim() LOL :D
        v.LoginExist = loginExist.text.Trim() == "exist";
    }

    static public IEnumerator BuyItem(Item item)
    {
        WWWForm form = new WWWForm();

        form.AddField("player_id", Cmn.player_id);
        form.AddField("item_id", item.ID);

        WWW buyItem = new WWW(buyItemURL, form);

        yield return buyItem;

        Debug.Log(buyItem.text);
        if (buyItem.text.Trim() != "confirm")
        {

        }
    }

    /// <summary>
    /// get login(nik), money, xp
    /// </summary>
    static public IEnumerator SelectPlayerInfo()
    {
        WWWForm form = new WWWForm();
        form.AddField("id", Cmn.player_id);

        WWW selectPlayerInfo = new WWW(selectPlayerInfoURL, form);

        yield return selectPlayerInfo;
        Debug.Log(selectPlayerInfo.text);
        string[] info = selectPlayerInfo.text.Split('|');

        Cmn.Nik = info[0];
        Cmn.Money = int.Parse(info[1]);
        Cmn.XP = int.Parse(info[2]);
    }

    /// <summary>
    /// parse DB and return initialized items
    /// </summary>
    /// <param name="player_id">need to find bought items</param>
    /// <returns>initialized items</returns>
    static public IEnumerator SelectItems(Shop shop)
    {
        //select rows (name, description, price(int), xp(int), item_type)
        //';' is separator between rows
        //pipeline ('|') is separator between columns 
        WWW selectItems = new WWW(selectItemsURL);

        yield return selectItems;
        Debug.Log(selectItems.text);
        string[] selectItemsList = (selectItems.text).Split(';');

        
        for (int i = 0; i < selectItemsList.Length - 1; i++)
        {
            string[] item = selectItemsList[i].Split('|');

            shop.allItemList.Add(new Item
            {
                ID = int.Parse(item[0]),
                Name = item[1],
                Description = item[2],
                Price = Convert.ToInt32(item[3]),
                
                //TODO convert xp to lvl
                Level = Convert.ToInt32(item[4]) / 100,
                Type = item[5],

                Image = null,
                IsBought = false
            });
        }
    }

    static public IEnumerator SelectPurchasedItems(Shop shop)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", Cmn.player_id);

        WWW purchasedItems = new WWW(selectPurchasedItemsURL, form);

        yield return purchasedItems;

        string[] purchasedItemsList = purchasedItems.text.Split(';');

        for (int i = 0; i < purchasedItemsList.Length - 1; i++)
            foreach (var item in shop.allItemList)
                if (item.Name == purchasedItemsList[i])
                    item.IsBought = true;
    }

    static public IEnumerator SelectWeapons(Data data)
    {
        //select rows (name, damage, area, distance, ammo, diffusing, AreaSizeDependsOnDistance)
        //';' is separator between rows
        //pipeline ('|') is separator between columns
        WWW selectWeapons = new WWW(weaponsURL);

        yield return selectWeapons;

        string[] selectItemsList = selectWeapons.text.Split(';');

        for (int i = 0; i < selectItemsList.Length - 1; i++)
        {
            string[] item = selectItemsList[i].Split('|');

            data.weapons.Add(new Weapon
            {
                Name = item[0],
                Damage = Convert.ToInt32(item[1]),
                Radius = Convert.ToInt32(item[2]),
                Distance = Convert.ToInt32(item[3]),
                Ammo = Convert.ToInt32(item[4]),
                Diffusing = Convert.ToBoolean(item[5]),
                AreaSizeDependsOnDistance = Convert.ToBoolean(item[6])
            });
        }
    }

    static public IEnumerator SelectGadgets(Data data)
    {
        //select rows (name, effect, area(radius), distance, turns, diffusing, AreaSizeDependsOnDistance)
        //';' is separator between rows
        //pipeline ('|') is separator between columns
        WWW selectGadgets = new WWW(gadgetsURL);

        yield return selectGadgets;

        string[] selectItemsList = selectGadgets.text.Split(';');

        for (int i = 0; i < selectItemsList.Length - 1; i++)
        {
            string[] item = selectItemsList[i].Split('|');

            data.gadgets.Add(new Gadget
            {
                Name = item[0],
                Effect = Convert.ToInt32(item[1]),
                Radius = Convert.ToInt32(item[2]),
                Distance = Convert.ToInt32(item[3]),
                Turns = Convert.ToInt32(item[4]),
                Diffusing = Convert.ToBoolean(item[5]),
                AreaSizeDependsOnDistance = Convert.ToBoolean(item[6])
            });
        }
    }

    static public IEnumerator SelectStatistic(Statistic statistic)
    {
        //select rows (statName, statValue)
        //';' is separator between rows
        //pipeline ('|') is separator between columns 
        WWWForm form = new WWWForm();
        form.AddField("player_id", Cmn.player_id);

        WWW selectStat = new WWW(selectStatURL, form);

        yield return selectStat;
        string[] selectStatList = (selectStat.text).Split(';');

        for (int i = 0; i < selectStatList.Length; i++)
        {
            string[] item = selectStatList[i].Split('|');

            statistic.statistic.Add(new PlayerStat
            {
                statName = item[0],
                statValue = item[1]
            });
        }
    }

    static public IEnumerator SelectLoadouts(Shop shop)
    {
        //select rows (statName, statValue)
        //';' is separator between rows
        //pipeline ('|') is separator between columns 
        WWWForm form = new WWWForm();
        form.AddField("player_id", Cmn.player_id);

        WWW selectLoadouts = new WWW(selectLoadoutsURL, form);

        yield return selectLoadouts;
        Debug.Log(selectLoadouts.text);
        string[] selectLoadoutsList = (selectLoadouts.text).Split(';');

        for (int i = 0; i < selectLoadoutsList.Length - 1; i++)
        {
            string[] item = selectLoadoutsList[i].Split('|');

            int weapon_id = int.Parse(item[1]);
            int gadget_id = int.Parse(item[2]);
            int skin_id = int.Parse(item[3]);
            int skill_id = int.Parse(item[4]);

            shop.loadoutsList.Add(new LoadoutInfo
            {
                currentLoadoutName = item[0],
                lastLoadoutName = item[0],
                weapon = shop.allItemList.Find(x => x.ID == weapon_id),
                gadget = shop.allItemList.Find(x => x.ID == gadget_id),
                skin = shop.allItemList.Find(x => x.ID == skin_id),
                skill = shop.allItemList.Find(x => x.ID == skill_id),
                isSynchronized = true
            });
        }
    }

    static public IEnumerator SynchronizeLoadouts(LoadoutInfo loadoutInfo)
    {
        WWWForm form = new WWWForm();

        form.AddField("currentLoadoutName", loadoutInfo.currentLoadoutName);
        form.AddField("lastLoadoutName", loadoutInfo.lastLoadoutName);
        form.AddField("weapon_id", loadoutInfo.weapon.ID);
        form.AddField("gadget_id", loadoutInfo.gadget.ID);
        form.AddField("skin_id", loadoutInfo.skin.ID);
        form.AddField("skill_id", loadoutInfo.skill.ID);
        form.AddField("player_id", Cmn.player_id);

        WWW synchronize = new WWW(synchronizeLoadoutsURL, form);

        yield return synchronize;

        Debug.Log(synchronize.error);
        Debug.Log(synchronize.text);
    }
}
