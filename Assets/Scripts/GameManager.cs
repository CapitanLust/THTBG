using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// There is 2 main state of play:
/// Decision  and  Performance
/// Decision    -- when player do his own turn and than confirm
/// Performance -- when all players confirms they turn and all avatars stars to 
///     execute action at same time
/// </summary>

public class GameManager : NetworkBehaviour {

    public Lobby lobby;
    public NetworkManager networkManager;

    public List<Player> players;

    public int readyPlayers = 0;

    Action onEachPlayerReady;

    public UI ui;

    void Awake()
    {
        lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        networkManager = lobby.netManager;
        //players = lobby.players;

        onEachPlayerReady = OnEachReady_Decision;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // TODO is it Ready and synced already?
        players = lobby.players;
        //unreadyPlayers = players.Count;
        foreach (var p in players)
        {
            // TODO to method
            p.gameManager = this; // TODO call to start
            p.ui = ui;
        }

        ui.UpdPlayerList(players);
    }


    #region Playing

    public void OnPlayerReady()
    {
        if ((++readyPlayers) == players.Count)
        {
            onEachPlayerReady();
            ResetReady();
        }
    }
    void ResetReady()
    {
        readyPlayers = 0;
        // auto-sync? it does on server, isn't it autosync there?
        // think it will, but didn't try yet
        foreach (var p in players) p.isReady = false;
    }


    void OnEachReady_Decision() // still server side
    {
        RpcPerform();
    }
    [ClientRpc]
    public void RpcPerform() // on each client
    {
        foreach (var p in players)
            p.Perform();
    }


    #endregion

    [Serializable]
    public class UI
    {
        public Text tx_log, tx_playerList;

        public void UpdPlayerList(List<Player> list)
        {
            tx_playerList.text = "";

            foreach (var p in list)
                tx_playerList.text += p.Nik + " ["
                    + (p.isReady ? "R]\n" : "  ]\n");
        }
    }

}
