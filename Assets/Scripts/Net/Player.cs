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
    /// that one -- is ready-call from decision
    /// </summary>
    /// <param name="serializedTurn"></param>
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

        foreach (var ta in turn.actions)
            ta.SyncNonSync(turn);

    }

    public void ReturnToInitialTurnState()
    {
        if(isAlive)
            avatar.ReturnToInitialTurnState();
    }

    public void Perform()
    {
        Debug.Log("Performing: " + Nik);
        ui.tx_log.text += "Performing " + Nik + "\n";

        update = Update_Game_Performance;
    }

    public void StartDecision()
    {
        turn.Clear(); // don't need on non-auth, but let it be just in case

        if (hasAuthority)
        {
            if (isAlive)
                turn.actions.Add(new MoveAction(turn));
            else       // TODO + ruler
                turn.actions.Add(new SpawnAction(turn));
            inputHandler = turn.actions[0].InputHandler;

            update = Update_Game_Decision;
        }
        else
            update = Update_Empty;

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
                // first, we need to disable upd performing

                CmdSetReady();
                CmdSyncTurn(turn.Serialized);

                ReturnToInitialTurnState();
            }
            else
            {
                inputHandler();
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
        if (!isReady && turn.Iterate())
        {
            if (hasAuthority)
            {
                isReady = true; // see comment \/ below
                CmdSetReady();
            }
            // others will wait for sync and just get true
            //  from turn.Iterate()

            // but also we either need to
            // check and set isReady. Because 
            // if for next frame, authorized will not synced yet
            // it will send another cmd request

            // sure, we can check sender within CmdSetReady()
            // but is it need? Hmm.. maybe it will improve performance to one check in every frame less TODO

            Debug.Log("Ended");
        }
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
public class SpawnAction : TurnAction, IUsingFloorCursor
{
    [NonSerialized]
    bool setted = false;

    public SpawnAction (Turn turn)
    {
        this.turn = turn;

        ActivateFCursor();
    }

    public override void InputHandler()
    {
        if (!setted)
        {
            // TODO check for availabilty of spawn. Like near physical borders and other player spawns
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Floor")
            {
                ControllingFCursor(hit.point);

                if (Input.GetMouseButtonDown(0))
                {
                    setted = true;
                    Point = hit.point;

                    ChangeFCursotState(GameManager.UI.FloorCursorState.Setted);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                setted = false;
                ChangeFCursotState(GameManager.UI.FloorCursorState.Deciding);
            }
        }
    }

    public override bool Action()
    {
        Debug.Log("SpawnInfo: " + Point);

        turn.Owner.gameManager.Spawn(Point, turn.Owner);

        DisableFCursor();

        return true; // TODO or GameManager.Spawn -> bool ?
    }

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
    }



    #region IFloorCursot implementation

    public void ActivateFCursor()
    {
        turn.Owner.ui.SetFloorCursorState(GameManager.UI.FloorCursorState.Deciding);
        turn.Owner.ui.SetActiveOfFloorCursor(true);
    }
    public void ControllingFCursor(Vector3 point)
    {
        turn.Owner.ui.SetFloorCursor(point);
    }
    public void ChangeFCursotState(GameManager.UI.FloorCursorState state)
    {
        turn.Owner.ui.SetFloorCursorState(state);
    }
    public void DisableFCursor()
    {
        turn.Owner.gameManager.ui.SetActiveOfFloorCursor(false);
    }

    #endregion


}
