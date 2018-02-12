using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour  {

    [SyncVar]
    public string Nik;

    [SyncVar]
    public Cmn.EPlayerColor Color;

    [SyncVar(hook = "Registrate")]
    public bool ReadyToRegistrate = false;

    [SyncVar] public bool isAlive = false;
    [SyncVar(hook = "OnIsReadyChanged")]
    public bool isReady = false;

    [SyncVar]
    public Batch batch;

    public GameManager gameManager;
    public GameManager.UI ui;

    #region Initiative part

    void Registrate(bool ready)
    {
        if(ReadyToRegistrate = ready)
            GameObject.Find("Lobby").GetComponent<Lobby>().AddPlayer(this);
    }
    
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // though object creates only on this scene. Anyway:
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            CmdSetMyParameters(Cmn.Nik, Cmn.PlayerColor);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("a1");

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            var lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
            if (ReadyToRegistrate && !lobby.players.Contains(this))
                lobby.AddPlayer(this);
        }

        DontDestroyOnLoad(gameObject);
    }

    

    [Command]
    void CmdSetMyParameters(string nik, Cmn.EPlayerColor color)
    {
        Nik = nik;
        Color = color;
        ReadyToRegistrate = true;
    }

    #endregion


    #region Playing

    // # START OF LOGIC
    [Command] // server side
    public void CmdSetReady()
    {
        isReady = true;
        // next -- execution goes to isReady sync hook (method)

        // still only server
        gameManager.OnPlayerReady();
    }
    public void OnIsReadyChanged(bool ready) // client side
    {
        isReady = ready; 
        ui.UpdPlayerList(gameManager.players);

        Debug.Log("check for repeating action");
    }

    public void Perform()
    {
        Debug.Log("Performing: " + Nik);
    }


    // input part

    private void Update()
    {
        if (!hasAuthority) return; // TODO another way?

        // Temp
        if (Input.GetKeyDown(KeyCode.R))
            CmdSetReady();
    }


    #endregion


}


[SerializeField]
public class Batch : IBatching
{
    public byte[] Serialized { get; set; }
}
interface IBatching
{
    byte[] Serialized { get; set; }
}


