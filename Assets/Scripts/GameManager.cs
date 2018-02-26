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
    public Player WePlayer; // TODO rename

    public bool isMatchStarted = false;

    public int readyPlayers = 0;

    Action onEachPlayerReady;

    public PlayerAvatar avatarPrefab;
    public UI ui;


    #region Initialization

    void Awake()
    {
        lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        networkManager = lobby.netManager;
        //players = lobby.players;

        onEachPlayerReady = OnEachReady_Decision;

        lobby.State = Lobby.LobbyState.Game;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // TODO is it Ready and synced already?
        players = lobby.players;
        //unreadyPlayers = players.Count;
        foreach (var p in players)
        {
            p.gameManager = this; // TODO call to start
            p.ui = ui;

            //if (p.hasAuthority) p.update = p.Update_Game_Decision; 

            if (p.hasAuthority)
            {
                WePlayer = p;
                //p.StartDecision();
            }
        }

        ResetReady();

        ui.UpdPlayerList(players);

        WePlayer.StartDecision();
    }
    #endregion

    #region Playing

    public void OnPlayerReady()
    {
        if ((++readyPlayers) == players.Count)
        {
            ResetReady();
            onEachPlayerReady();
        }
    }
    void ResetReady()
    {
        readyPlayers = 0;

        foreach (var p in players)
        {
            p.isReady = false;

            /* Moved to Player.StartDecision
            p.turn.Clear();
            if (p.isAlive) 
                p.turn.actions.Add(new MoveAction(p.turn));
            else       // TODO + ruler
                p.turn.actions.Add(new SpawnAction(p.turn));
            p.inputHandler = p.turn.actions[0].InputHandler;*/
        }
    }


    void OnEachReady_Decision() // still server side
    {
        onEachPlayerReady = OnEachReady_Performance;
        RpcPerform();
        //TODO process turns (kills, etc)
    }
    void OnEachReady_Performance()
    {
        RpcStartDecision();
        onEachPlayerReady = OnEachReady_Decision;
    }

    [ClientRpc]
    public void RpcPerform() // on each client
    {
        foreach (var p in players)
            p.Perform();
    }
    [ClientRpc]
    public void RpcStartDecision() // on each client
    {
        //foreach (var p in players)
            //p.StartDecision();
        WePlayer.StartDecision(); // only authority player
    }


    #endregion


    // called on all clients
    public void Spawn(Vector3 spawnPos, Player player)
    {
        var avatar = Instantiate(avatarPrefab) as PlayerAvatar;

        player.avatar = avatar; // TODO change linking logic?
        avatar.player = player; // or is it quite enough?

        //avatar.transform.SetParent(player.transform);
        avatar.transform.position = spawnPos + new Vector3(0,0.6f,0); // TODO do high offset in prefab

        player.isAlive = true;

        // TODO apply settings
        var renderer = avatar.transform.GetChild(0).GetComponent<Renderer>();
        renderer.materials[0].color = player.Color.ToColor();

        if (player.hasAuthority) ui.ClearSpawnPoint(); //TODO point to common ui points per turns
    }


    [Serializable]
    public class UI
    {
        public Text tx_log, tx_playerList;
        public RawImage img_SpawnPoint;

        public void UpdPlayerList(List<Player> list)
        {
            tx_playerList.text = "";

            foreach (var p in list)
                tx_playerList.text += p.Nik + " ["
                    + (p.isReady ? "R]\n" : "  ]\n");
        }


        public void MarkSpawnPoint(Vector2 pos)
        {
            img_SpawnPoint.rectTransform.position = pos;
            img_SpawnPoint.gameObject.SetActive(true);
        }
        public void ClearSpawnPoint()
        {
            img_SpawnPoint.gameObject.SetActive(false);
        }

    }

}
