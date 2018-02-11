using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour {

    public NetworkManager netManager;
    public Player playerPrefab;

    public List<Player> players;

    public LobbyUI ui;



    private void Awake()
    {
        netManager = GameObject.Find("Network Manager").GetComponent<NetworkManager>();

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        ui.btn_StartGame.gameObject.SetActive(true);
    }


    public void StartGame()
    {
        netManager.ServerChangeScene("Battlefield");
    }

    public void AddPlayer(Player p)
    {
        players.Add(p);
        ui.UpdateTxServerList();        
    }

    
}
