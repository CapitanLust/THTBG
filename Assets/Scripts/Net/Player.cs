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
    public Action inputHandler 
        = () => { };

    public PlayerAvatar avatar;


    public GameManager gameManager;
    public GameManager.UI ui;

    #region Initiative part

    private void Awake()
    {
        update = Update_Empty;
        turn.Owner = this;
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
    /// <summary>
    /// that one -- is ready call from decision
    /// </summary>
    /// <param name="serializedTurn"></param>
    /*[Command] // server side
    public void CmdSetReady(byte[] serializedTurn)
    {
        isReady = true;
        // next -- execution goes to isReady sync hook (method)
        RpcSyncTurn(serializedTurn);
        // still only server
        gameManager.OnPlayerReady();
    }*/

    [Command] // server side
    public void CmdSetReady()
    {
        isReady = true;
        gameManager.OnPlayerReady();
    }
    public void OnIsReadyChanged(bool ready) // client side
    {
        isReady = ready;
        ui.UpdPlayerList(gameManager.players);
    }

    [Command]
    public void CmdSyncTurn(byte[] serializedTurn)
    {
        RpcSyncTurn(serializedTurn);
    }
    
    
    [ClientRpc]
    public void RpcSyncTurn(byte[] barray)
    {
        turn = Turn.Deserialize(barray) as Turn;
        turn.Owner = this; //       Important!  

    }

    public void Perform()
    {
        Debug.Log("Performing: " + Nik);
        ui.tx_log.text += "Performing " + Nik + "\n";

        update = Update_Game_Performance;

        turn.Perform();
    }

    public void StartDecision()
    {
        if(turn.actions.Count>0)
        Debug.Log(turn.actions[0].Point);
        turn.Clear();

        if (isAlive)
            turn.actions.Add(new MoveAction(turn));
        else       // TODO + ruler
            turn.actions.Add(new SpawnAction(turn));
        inputHandler = turn.actions[0].InputHandler;

        update = Update_Game_Decision;

        Debug.Log(turn.actions[0].Point);
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
    public void Update_Game_Decision()
    {
        if (Input.GetKeyDown(KeyCode.K)) Debug.Log(""); // reserved for debug

        if (!isReady) 
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                CmdSyncTurn(turn.Serialized);
                CmdSetReady();
            }
            else
            {
                inputHandler();

                /*if (!isAlive)
                {
                    ListenMouseForSpawn();
                }
                else
                    if(avatar!=null) // check spawn delays
                        avatar.update();
                        */
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                //turn = null;
                //turn.Clear();                                       ////////TODO///////////
                //avatar.Cancel();
            }
        }
    }
    public void Update_Game_Performance()
    {
        
    }


    /*
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

    public void SetSpawn(Vector3 spawnPos)
    {
        TurnAction spawnAct = new SpawnAction() { Point = spawnPos , turn = turn };
        turn.actions.Add(spawnAct);

        // yes, gettin mousePos from here is declining some universallity, but idc
        ui.MarkSpawnPoint(Input.mousePosition);
    }*/

    #endregion


    public void OnStartMatch ()
    {

    }


    #endregion


    #region UI
    

    #endregion


}



[Serializable]
public class SpawnAction : TurnAction
{
    public SpawnAction (Turn turn) { this.turn = turn; }

    public override void InputHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // TODO check for availabilty of spawn. Like near physical borders and other player spawns
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Floor")
            {
                //turn.Owner.SetSpawn(hit.point);

                Point = hit.point;

                turn.Owner.ui.MarkSpawnPoint(Input.mousePosition);
            }
        }
    }

    public override bool Action()
    {
        Debug.Log("SpawnInfo: " + Point);

        turn.Owner.gameManager.Spawn(Point, turn.Owner);

        return true; // TODO or GameManager.Spawn -> bool ?
    }
}
