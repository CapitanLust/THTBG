using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

public class Player : NetworkBehaviour {

    [SyncVar]
    public string Nik;

    [SyncVar]
    public Cmn.EPlayerColor Color;

    [SyncVar(hook = "Registrate")]
    public bool ReadyToRegistrate = false;

    [SyncVar] public bool isAlive = false;
    [SyncVar(hook = "OnIsReadyChanged")]
    public bool isReady = false;

    public Turn turn = new Turn();


    public bool performing = false;

    public Action update;

    public PlayerAvatar avatar;


    public GameManager gameManager;
    public GameManager.UI ui;

    #region Initiative part

    private void Awake()
    {
        update = Update_Empty;
    }

    void Registrate(bool ready)
    {
        if (ReadyToRegistrate = ready)
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
    public void CmdSetReady(byte[] serializedTurn)
    {
        isReady = true;
        // next -- execution goes to isReady sync hook (method)

        // sync turn
        RpcSyncTurn(serializedTurn);
        // а, нет. Это не было нормально. Все-таки нужно вручную синхр-ть

        // still only server
        gameManager.OnPlayerReady();
    }
    public void OnIsReadyChanged(bool ready) // client side
    {
        isReady = ready;
        ui.UpdPlayerList(gameManager.players);
    }
    
    
    [ClientRpc]
    public void RpcSyncTurn(byte[] barray)
    {
        turn = Turn.Deserialize(barray) as Turn;
        turn.Owner = this; //       Important!  

        //foreach (var ta in turn.actions) // hz
        //ta.turn = turn;

        Debug.Log("SynTurn " + Nik + ". turn is SI = " + (turn.actions[0] is SpawnInfo));
    }

    public void Perform()
    {
        Debug.Log("Performing: " + Nik);
        ui.tx_log.text += "Performing " + Nik + "\n";

        if (turn != null)
            turn.Perform();
    }


    #region input

    void Update()
    {
        update();
    }

    public void Update_Empty() { }
    public void Update_Lobby()
    {

    }
    public void Update_Game()
    {
        // todo check 'if' performance
        if (Input.GetKeyDown(KeyCode.D)) Debug.Log(turn == null); // reserved for debug

        if (!isReady) 
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                CmdSetReady(turn.Serialized);
            }
            else
            {
                if (!isAlive)
                {
                    ListenMouseForSpawn();
                }
                else
                    if(avatar!=null) // check spawn delays
                        avatar.update();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                turn = null;
                avatar.Cancel();
            }
        }
    }

    void ListenMouseForSpawn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // TODO check for availabilty of spawn. Like near physical borders and other player spawns
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Floor")
                SetSpawn(hit.point);
        }
    }
    void SetSpawn(Vector3 spawnPos)
    {
        TurnAction spawnAct = new SpawnInfo() { Point = spawnPos , turn = turn };
        turn.actions.Add(spawnAct);

        // yes, gettin mousePos from here is declining some universallity, but idc
        ui.MarkSpawnPoint(Input.mousePosition);
    }

    #endregion


    public void OnStartMatch ()
    {

    }


    #endregion


    #region UI
    

    #endregion


}



[Serializable]
public class SpawnInfo : TurnAction
{
    public override bool Action()
    {
        Debug.Log("SpawnInfo: " + Point);

        turn.Owner.gameManager.Spawn(Point, turn.Owner);

        return true; // TODO or GameManager.Spawn -> bool ?
    }
}
