using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : UnityEngine.Networking.NetworkManager {

    //public List<Player> players = new List<Player>();

    public Action<NetworkConnection> onPlayerAdded;

    #region Handling singleton

    public static NetworkManager s_Instance
    {
        get;
        protected set;
    }

    protected virtual void Awake()
    {
        if (s_Instance != null) Destroy(gameObject);
        else s_Instance = this;
    }
    protected virtual void OnDestroy()
    {
        if (s_Instance == this) s_Instance = null;
    }
    #endregion
    
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);  //   don't. We want to control prefabs

        //Player newPlayer = Instantiate(playerPrefab); // + as or <> ?
        //DontDestroyOnLoad(newPlayer);
        //NetworkServer.AddPlayerForConnection(conn, newPlayer.gameObject, playerControllerId);

        Debug.Log("Player connected");
        //RpcAddPlayerToList(conn);

        if (onPlayerAdded != null) onPlayerAdded(conn);
    }


    /*/// <see cref="https://answers.unity.com/questions/1137966/onserveraddplayer-is-not-called.html"/>
    public override void OnClientConnect(NetworkConnection conn)
    {
        //ClientScene.AddPlayer(conn, 0);
        base.OnClientConnect(conn);
        Debug.Log("Asd");
        //Debug.Log(conn.clientOwnedObjects.Count);
    }*/

}
