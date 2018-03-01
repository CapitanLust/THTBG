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

        onEachPlayerReady = OnEachReady_Decision;

        lobby.State = Lobby.LobbyState.Game;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // TODO is it Ready and synced already?
        players = lobby.players;
        foreach (var p in players)
        {
            p.gameManager = this; // TODO call to start
            p.ui = ui;

            if (p.hasAuthority)
                WePlayer = p;
        }

        ResetReady();

        ui.UpdPlayerList(players);

        WePlayer.StartDecision();
    }
    #endregion

    #region Playing

    public void OnPlayerReady() // only on server
    {
        if ((++readyPlayers) == players.Count)
            StartCoroutine( Cmn.AwaitAnd(() => {
                ResetReady();
                onEachPlayerReady();
            }));
        // delay for await of turn sync
        // TODO comment /\
    }
    
    void ResetReady()
    {
        readyPlayers = 0;

        foreach (var p in players)
            p.isReady = false;
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

        foreach (var p in players)
            p.StartDecision();

    }


    #endregion


    // called on all clients
    public void Spawn(Vector3 spawnPos, Player player)
    {
        var avatar = Instantiate(avatarPrefab) as PlayerAvatar;

        player.avatar = avatar; // TODO change linking logic?
        avatar.player = player; // or is it quite enough?

        avatar.transform.position = spawnPos;

        player.isAlive = true;

        // TODO apply settings
        var renderer = avatar.transform.GetChild(0).GetComponent<Renderer>();
        renderer.materials[0].color = player.Color.ToColor();

        //if (player.hasAuthority) ui.ClearSpawnPoint(); //TODO point to common ui points per turns
    }


    [Serializable]
    public class UI
    {
        public Text tx_log, tx_playerList;
        public RawImage img_SpawnPoint;

        public SpriteRenderer FloorCursor;

        public FloorCursorColors floorCursorColors;
        [Serializable]
        public struct FloorCursorColors
        {
            /// <summary> Color of sprite-cursor on the floor,
            /// that appear when we're deciding point </summary>
            public Color Decision;
            /// <summary> Color of sprite-cursor when we've select and we are ready </summary>
            public Color Setted;
            /// <summary> Color of sprite-cursor when we've point to place where we can't do Action </summary>
            public Color Unavailable;
        }

        public void UpdPlayerList(List<Player> list)
        {
            tx_playerList.text = "";

            foreach (var p in list)
                tx_playerList.text += p.Nik + " ["
                    + (p.isReady ? "R]\n" : "  ]\n");
        }
        
        [Obsolete("Use FloorCursor instead", true)]
        public void MarkSpawnPoint(Vector2 pos)
        {
            img_SpawnPoint.rectTransform.position = pos;
            img_SpawnPoint.gameObject.SetActive(true);
        }
        [Obsolete("Use FloorCursor instead", true)]
        public void ClearSpawnPoint()
        {
            img_SpawnPoint.gameObject.SetActive(false);
        }


        public void SetFloorCursor (Vector3 pointOnFloor)
        {
            FloorCursor.transform.position = pointOnFloor
                + new Vector3(0f,0.01f,0f);
            // TODO rotation and apply Cursor of certain Equipment
        }
        public void SetActiveOfFloorCursor(bool active)
        {
            FloorCursor.gameObject.SetActive(active);
        }
        public void SetFloorCursorState(FloorCursorState state)
        {
            switch (state)
            {
                case FloorCursorState.Deciding: FloorCursor.color = floorCursorColors.Decision; break;
                case FloorCursorState.Setted: FloorCursor.color = floorCursorColors.Setted; break;
                case FloorCursorState.Unavailable: FloorCursor.color = floorCursorColors.Unavailable; break;
            }
        }
        public enum FloorCursorState { Deciding, Setted, Unavailable };

    }

}
