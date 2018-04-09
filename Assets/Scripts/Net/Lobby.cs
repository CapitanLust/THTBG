using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour {
    
    public Player playerPrefab;

    public List<Player> players;    

    public enum LobbyState { Lobby, Game }
    public LobbyState State = LobbyState.Lobby; // TODO returning from game
    
    public LobbyUI ui;

    #region Handling singleton

    public static Lobby s_Instance
    {
        get;
        protected set;
    }

    protected virtual void Awake()
    {
        if (s_Instance != null) Destroy(gameObject);
        else
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    protected virtual void OnDestroy()
    {
        if (s_Instance == this) s_Instance = null;
    }
    #endregion


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            ui = GameObject.Find("Lobby UI").GetComponent<LobbyUI>();
            ui.OnSceneChangedFromLobby(this);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        ui.btn_StartGame.gameObject.SetActive(true); // OnStartServer here is called only on "Lobby" scene
    }

    public void StartGame()
    {
        NetworkManager.singleton.ServerChangeScene("Battlefield");
    }

    public void ExitMatch()
    {
        NetworkManager.singleton.ServerChangeScene("Lobby");
        State = LobbyState.Lobby;
    }

    public void AddPlayer(Player p)
    {
        players.Add(p);

        if (p.hasAuthority) p.update = p.Update_Lobby; // TODO migrate to Player.cs?

        ui.UpdateTxServerList(); 
    }

    public Player GetPlayerByNik(string nik)
    {
        foreach (var p in players) if (nik == p.Nik) return p;
        return null;
    }

    
}
