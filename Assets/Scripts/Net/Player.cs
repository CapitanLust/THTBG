using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Player : NetworkBehaviour  {

    [SyncVar]
    public string Nik;

    [SyncVar]
    public Cmn.EPlayerColor Color;

    [SyncVar(hook = "Registrate")]
    public bool ReadyToRegistrate = false;

    [SyncVar] public bool isAlive = false;
    [SyncVar] public bool isReady = false;

    [SyncVar]
    public Batch batch;

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

    

    #endregion


}

[SerializeField]
public class Batch
{

}