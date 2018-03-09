using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Menu : MonoBehaviour {

    //  Temp Menu logic:
    //   direct connection
    //   data in static script 
    //    and no files for 1 machine debugging

    public NetworkManager netManager;

    void Start()
    {
        netManager = GameObject.Find("Network Manager").GetComponent<NetworkManager>();

        // for debug
        ui.sl_PlayerColor.value = UnityEngine.Random.Range(0, 5);
        ui.inp_Nik.text = "d-" + UnityEngine.Random.Range(100, 999);

        OnInpNikChanged(); OnSliderColorChanged(); // forced calls
        OnDropDownWeaponChanged();

    }

    public void Create()
    {
        netManager.StartHost();
    }

    public void Connect()
    {
        netManager.networkAddress = ui.inp_ip.text;
        netManager.StartClient();
    }

    #region UI part

    public UIContainer ui;

    [Serializable]
    public class UIContainer
    {
        public InputField inp_Nik, inp_ip;
        public Slider sl_PlayerColor;
        public Text tx_PlayerColor;
        public Dropdown drop_Weapon;
    }
    
    public void OnInpNikChanged()
    {
        Cmn.Nik = ui.inp_Nik.text;
    }

    public void OnDropDownWeaponChanged()
    {
        Cmn.Weapon = ui.drop_Weapon.captionText.text;
    }

    public void OnSliderColorChanged()
    {
        Cmn.EPlayerColor playerColor = (Cmn.EPlayerColor) Convert.ToInt32( ui.sl_PlayerColor.value );

        Cmn.PlayerColor = playerColor;

        // each time it's painful that unity is not supports c#6.0> features :(
        ui.tx_PlayerColor.text = "Color: <b><Color=" 
            + playerColor.ToHex() + ">"
                + playerColor.ToString()
            + "</Color></b>";
    }

    #endregion


}
