using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour {

    public NetworkManager netManager;
    public Player playerPrefab;

    public List<Player> players;

    private void Awake()
    {
        netManager = GameObject.Find("Network Manager").GetComponent<NetworkManager>();

        DontDestroyOnLoad(gameObject);
    }

        
    public void AddPlayer(Player p)
    {
        players.Add(p);
        ui_UpdateTxServerList();        
    }

    #region UI part
    // TODO Lobby_UI to external script!

    void ui_UpdateTxServerList()
    {
        ui.tx_PlayerList.text = "Players:\n";

        foreach (var p in players)
            ui.tx_PlayerList.text += p.Nik
                + " <Color=" + p.Color.ToHex() + ">◘</Color>\n";
    }

    

    public UIContainer ui;

    [Serializable]
    public class UIContainer
    {
        public Text tx_PlayerList;
    }

    #endregion
}
