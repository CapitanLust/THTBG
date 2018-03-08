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

    public GameRuler ruler; 

    public List<Player> players;
    public Player WePlayer; 

    //public List<PlayerAvatar> avatars; TODO
    //public PlayerAvatar WeAvatar;

    public bool IsMatchStarted = false;

    public int readyPlayers = 0;

    Action onEachPlayerReady;

    public PlayerAvatar avatarPrefab;
    public UI ui;

    Data data;


    #region Initialization

    void Awake()
    {
        data = GameObject.Find("Data").GetComponent<Data>();

        lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        networkManager = lobby.netManager;

        onEachPlayerReady = OnEachReady_Decision;

        lobby.State = Lobby.LobbyState.Game;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        players = lobby.players;
        foreach (var p in players)
        {
            p.gameManager = this; 
            p.ui = ui;

            if (p.hasAuthority)
                WePlayer = p;
        }

        ResetReady();

        ui.UpdPlayerList(players);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        // TODO nail with desync of OnStartClient and OnStartServer
        StartCoroutine(WaitAnd(() => {
            CmdStartMatch();
        }, .5f));
    }
    IEnumerator WaitAnd(Action whatNext, float awaitTime)
    {
        yield return new WaitForSeconds(awaitTime);
        whatNext();
    }

    [Command]
    public void CmdStartMatch()
    {
        RpcOnStartMatch();
    }



    #endregion

    #region Playing

    public void OnPlayerReady() // only on server
    {
        if ((++readyPlayers) == ruler.CountOfNotDead)
            StartCoroutine( WaitAnd(() => {
                ResetReady();
                onEachPlayerReady();
            }, .5f));

        // TODO via callback
        
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
        if (!ruler.CheckMatchForEnd())
        {
            RpcStartDecision();
            onEachPlayerReady = OnEachReady_Decision;
        }
        else
            EndMatch();
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


    public void ExitMatch()
    {

    }

    
    [ClientRpc]
    public void RpcOnStartMatch()
    {
        IsMatchStarted = true;
        WePlayer.StartDecision();
    }

    public void EndMatch()
    {
        RpcOnEndMatch();
        ui.btn_ExitMatch.SetActive(true);
    }
    [ClientRpc]
    public void RpcOnEndMatch()
    {
        OnEndMatch();
    }
    public void OnEndMatch()
    {
        IsMatchStarted = false;
        foreach (var p in players)
            p.Stop();

        // temp score summary
        ui.tx_playerList.text = "Match ended\n";
        foreach (var p in players)
            ui.tx_playerList.text += p.Nik + "  " + p.XP + "xp\n";
    }

    [Command]
    public void CmdAwardKiller(string nik, float successQ)
    {
        var killer = lobby.GetPlayerByNik(nik);
        killer.XP += (int)(successQ * 10f);

        if (WePlayer == killer)
            ui.Killmarker();
    }

    #endregion


    [Command]
    public void CmdSpawn(Vector3 spawnPos, NetworkInstanceId instanceId)
    {
        var player = NetworkServer.FindLocalObject(instanceId).GetComponent<Player>();
        var avatar = Instantiate(avatarPrefab);

        NetworkServer.SpawnWithClientAuthority(
            avatar.gameObject, player.gameObject);

        RpcInstantiateSpawned(spawnPos, instanceId, avatar.netId);
    }

    [ClientRpc]
    public void RpcInstantiateSpawned(Vector3 spawnPos, NetworkInstanceId instanceId, NetworkInstanceId avatarInstanceId)
    {
        var player = ClientScene.FindLocalObject(instanceId).GetComponent<Player>();
        var avatar = ClientScene.FindLocalObject(avatarInstanceId).GetComponent<PlayerAvatar>();

        player.avatar = avatar;
        avatar.player = player;

        avatar.transform.position = spawnPos;

        // TODO apply settings
        var renderer = avatar.transform.GetChild(0).GetComponent<Renderer>();
        renderer.materials[0].color = player.Color.ToColor();

        player.loadout.Inflate(avatar, data);

        player.isAlive = true;
    }


    /*public void AddAvatar(PlayerAvatar avatar, string ownerNik)
    {
        foreach (var spawnedAvatar in avatars)
            if (spawnedAvatar.player.Nik == ownerNik)
                return; // already spawned by another client

        avatars.Add(avatar);
    }*/



    [Serializable]
    public class UI
    {
        public GameManager gameManager;
        public Text tx_log, tx_playerList,
                    tx_weapon_ammo;
        public RawImage img_SpawnPoint;
        public GameObject btn_ExitMatch;

        public Transform FloorCursorAnch;
        public SpriteRenderer FloorCursor;

        public Animator animHitmarker;

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
                tx_playerList.text += (gameManager.WePlayer == p ? "| " : "  ")
                    + p.Nik + " ["
                    + (p.isReady ? "R] " : "  ] ")
                    + (p.isAlive ? p.avatar.HP + "hp\n" : "\n");
        }
        

        public void SetFloorCursor (Vector3 pointOnFloor)
        {
            FloorCursorAnch.transform.position = pointOnFloor;
            // TODO rotation and apply Cursor of certain Equipment
        }
        public void SetActiveOfFloorCursor(bool active)
        {
            FloorCursorAnch.gameObject.SetActive(active);
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


        public void Hitmarker()
        {
            animHitmarker.SetTrigger("hitmarker");
        }
        public void Killmarker()
        {
            animHitmarker.SetTrigger("killmarker");
        }

        public void UpdateWeaponPanel_Ammo(WeaponHandler weaponHandler)
        {
            tx_weapon_ammo.text = weaponHandler.info.Name + ": " + weaponHandler.CurMag;
        }
    }

}
