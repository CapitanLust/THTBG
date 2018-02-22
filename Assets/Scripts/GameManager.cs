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
            // TODO to method
            p.gameManager = this; // TODO call to start
            p.ui = ui;

            if (p.hasAuthority) p.update = p.Update_Game; 
        }

        ui.UpdPlayerList(players);
    }
    #endregion

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


    // called on all clients
    public void Spawn(Vector3 spawnPos, string playerNik)
    {
        var avatar = Instantiate(avatarPrefab) as PlayerAvatar;
        var player = lobby.GetPlayerByNik(playerNik);

        player.avatar = avatar; // TODO change linking logic?
        avatar.player = player; // or is it quite enough?

        //avatar.transform.SetParent(player.transform);
        avatar.transform.position = spawnPos + new Vector3(0,0.6f,0); // TODO do high offset in prefab

        player.isAlive = true;

        // TODO apply settings
        var meshRenderer = avatar.transform.GetChild(0).GetComponent<MeshRenderer>();
        var mat = meshRenderer.material; // copy
        mat.color = player.Color.ToColor();
        meshRenderer.material = mat;


        if (player.hasAuthority) ui.ClearSpawnPoint();
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
