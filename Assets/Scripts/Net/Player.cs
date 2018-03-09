using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

public class Player : NetworkBehaviour, IInitialState {

    [SyncVar]
    public string Nik;

    [SyncVar]
    public Cmn.EPlayerColor Color;

    [SyncVar]
    public int XP;

    [SyncVar(hook = "Registrate")]
    public bool ReadyToRegistrate = false;

    [SyncVar] public bool isAlive = false;
    [SyncVar] public bool isDead = false; // seems similar with isAlive. But isDead -- is only after Avatar's Death
    [SyncVar(hook = "OnIsReadyChanged")]
    public bool isReady = false;
    public bool localUpdReady = false; // for stop performing update before server sync isReady

    public Turn turn = new Turn();


    public bool performing = false;

    public Action update;       
    public Action inputHandler 
        = () => { };

    public PlayerAvatar avatar;

    [SyncVar]
    public Loadout loadout = new Loadout();


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

    public void Stop()
    {
        update = Update_Empty;
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // though object creates only on this scene. Anyway:
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            CmdSetMyParameters(Cmn.Nik, Cmn.PlayerColor);
            CmdSetLoadout(Cmn.Weapon, "", "");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

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
        // they will be synced
        Nik = nik;
        Color = color;
        ReadyToRegistrate = true;
    }
    [Command]
    void CmdSetLoadout(string weaponName, string gadget1Name, string gadget2Name)
    {
        RpcSyncLoadout(weaponName, gadget1Name, gadget2Name);        
    }
    [ClientRpc] // again rpc because of sync-ing class
    void RpcSyncLoadout (string weaponName, string gadget1Name, string gadget2Name)
    {
        loadout.WeaponName = weaponName;
        loadout.Gadget1Name = gadget1Name;
        loadout.Gadget2Name = gadget2Name;
    }

    #endregion

    

    #region Playing

    // # START OF LOGIC

    /// <summary>
    /// that one -- is ready-call from decision
    /// </summary>
    [Command(channel = 0)] // server side
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

    [Command(channel = 0)]
    public void CmdSyncTurn(byte[] serializedTurn)
    {
        RpcSyncTurn(serializedTurn);
    }
    
    
    [ClientRpc(channel = 0)]
    public void RpcSyncTurn(byte[] barray)
    {
        turn = Turn.Deserialize(barray) as Turn;
        turn.Owner = this; //       Important!  

        foreach (var ta in turn.actions)
            ta.SyncNonSync(turn);

    }


    public void Perform()
    {
        turn.Performing = true;
        localUpdReady = false;
        update = Update_Game_Performance;
    }

    public void StartDecision()
    {
        turn.Clear(); // don't need on non-auth, but let it be just in case
        turn.Performing = false;

        if (hasAuthority)
        {
            FixateState();

            if (isAlive)
                turn.actions.Add(new MoveAction(turn));
            else       
                if(gameManager.ruler.CanRevive(this))
                    turn.actions.Add(new SpawnAction(turn));

            inputHandler = turn.actions[0].InputHandler;

            update = Update_Game_Decision;
        }
        else
            update = Update_Empty;

    }


    public void SetInputHandlerOnLast()
    {
        inputHandler = turn.actions[turn.actions.Count - 1].InputHandler;
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
            if (Input.GetKeyDown(KeyCode.E))
            {
                CmdSyncTurn(turn.Serialized);

                ReturnToInitialTurnState();
                gameManager.ui.SetActiveOfFloorCursor(false);

                CmdSetReady();
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
        if (!localUpdReady && !isReady && turn.Iterate())
        {
            localUpdReady = true;
            if (hasAuthority)
                CmdSetReady();
        }
    }

    [Command]
    public void CmdCallSpawn(Vector3 point)
    {
        gameManager.CmdSpawn(point, netId);
        // we need to go thru player first, to send command from server auth
        // ( TurnAction -> Player(s) -> GameManager (s) )
    }
    
    #endregion

    #region Playtrouhg other networking


    #endregion

    public void OnStartMatch ()
    {

    }


    #endregion


    #region UI


    #endregion

    #region IInitialState implementation

    public void FixateState()
    {
        if (isAlive)
        {
            avatar.FixateState();
            loadout.FixateState();
        }
    }
    
    public void ReturnToInitialTurnState()
    {
        if (isAlive)
        {
            avatar.ReturnToInitialTurnState();
            loadout.ReturnToInitialTurnState();
        }
    }

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

                    Confirmed = true;

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
        if (turn.Owner.hasAuthority)
            turn.Owner.CmdCallSpawn(Point);

        DisableFCursor();

        return true;
    }

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
    }



    #region IFloorCursot implementation

    public void ActivateFCursor()
    {
        if (turn.Performing) return;

        var diameter = turn.Owner.gameManager.avatarPrefab.Radius * 2;
        turn.Owner.ui.FloorCursorAnch.localScale
               = new Vector3(diameter, 1, diameter);
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
