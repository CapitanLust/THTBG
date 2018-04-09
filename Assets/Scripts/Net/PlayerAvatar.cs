using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class PlayerAvatar : NetworkBehaviour, IInitialState {

    public Player player;

    public float speed = 0.1f;
    public float Radius = .5f;
    public bool isTurning = false,
                isCommonTurning = false;

    [SyncVar]
    public float HP = 100;


    public Animator anim;
    public Rigidbody rg;
    public Transform weaponTransformContainer;

    Vector3 feetFacing; /// for anim synces
    float fakeVelocity; //|
    Vector3 prevPos;    //|

    void Awake()
    {
        feetFacing = transform.forward;
        prevPos = transform.position;
    }

    public void update() // notice that there's lowercase 'u'
    {               // bcse call goes from Player
        IterateTurn();
    }

    public void LateUpdate()
    { 
        SyncAnimator();
    }

    public void SyncAnimator()
    {
        fakeVelocity = (prevPos - transform.position).magnitude;
        prevPos = transform.position;

        float b = (Mathf.Atan2(feetFacing.z, feetFacing.x) - Mathf.Atan2(transform.forward.z, transform.forward.x))
            * Mathf.Rad2Deg;
        if (b > 180) b = -360 + b;


        anim.SetFloat("Velocity", fakeVelocity);
        anim.SetFloat("Feet Angle", b);
        anim.SetFloat("Feet Angle mod", b % 60);
        

        if (fakeVelocity < 0.3f)
        {
            if (b > 60 || b < -60) // not calling abs
            {
                feetFacing = transform.forward;
                anim.SetTrigger("Restep");
            }
        }
    }

    #region Avatar's functional methods

    /// <param name="toward"> Requiers already normilized toward! </param>
    public void Move (Vector3 toward)
    {
        feetFacing = toward.normalized;
        transform.position += toward * speed;
    }

    public void LookAt (Vector3 point)
    {
        var difNorm = (point - transform.position).normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        if(Mathf.Abs(angle)>0.05f)
            transform.rotation = Quaternion.Euler(0f, 90f - angle, 0f);
    }
    public void LookAtDir (Vector3 direction)
    {
        var difNorm = direction.normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 90f - angle, 0f);
    }

    /// <returns> Angle beetwen facing and dir are enough small </returns>
    public bool LookAtSmooth(Vector3 point)
    {
        var difNorm = (point - transform.position).normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        Quaternion quatTo = Quaternion.Euler(0f, 90f - angle, 0f);
        transform.rotation = Quaternion.Lerp(
                transform.rotation,
                quatTo,
                .23f
            );

        return Mathf.Abs(Quaternion.Angle(transform.rotation, quatTo)) < 0.01f;
    }
    /// <returns> Angle beetwen facing and dir are enough small </returns>
    public bool LookAtDirSmooth (Vector3 direction)
    {
        var difNorm = direction.normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        Quaternion quatTo = Quaternion.Euler(0f, 90f - angle, 0f);
        transform.rotation = Quaternion.Slerp(
                transform.rotation,
                quatTo,
                .23f
            );

        return Mathf.Abs(Quaternion.Angle(transform.rotation, quatTo)) < 0.01f;
    }

    #endregion


    // User Part

    Vector3 initialTurnPos;
    
    public void StartTurning()
    {

        initialTurnPos = transform.position;

        isCommonTurning = false;
        isTurning = true;
    }

    public void IterateTurn()
    {
        //if (player.turn.Iterate())
        //{
        //    //isCommonTurning = false;
        //    player.CmdSetReady();
        //    Debug.Log("Ended");
        //}
    }

    public void ResetToInitial()
    {
        transform.position = initialTurnPos;
    }
    

    public void Cancel()
    {
        ResetToInitial();
        //turn.Clear();
    }


    public void StartCommonTurning()
    {
        isTurning = false;
        isCommonTurning = true;
    }



    [Command]
    public void CmdSendDamage(NetworkInstanceId victimAvatarID, float damage, float successQ)
    {
        var victim = NetworkServer.FindLocalObject(victimAvatarID).GetComponent<PlayerAvatar>();
        victim.CmdGetDamage(damage, player.Nik, successQ);
    }

    [Command]
    public void CmdGetDamage (float damage, string hitterNik, float successQ)
    {
        HP -= damage;
        RpcUpdList();

        if (HP <= 0)
        {
            if (player.gameManager.ruler.CanRevive(player))
                player.CmdSetReady();

            player.gameManager.CheckPlayersReady(); // if player dies after all is ready

            RpcDie();

            player.gameManager.CmdAwardKiller(hitterNik, successQ);
        }
    }
    [ClientRpc]
    public void RpcUpdList()
    {
        player.gameManager.ui.UpdPlayerList(player.gameManager.players);
    }
    
    [ClientRpc]
    public void RpcDie()
    {
        player.update = player.Update_Empty;        

        player.isAlive = false;
        player.isDead = true;
        player.avatar = null;
        //Remove from avatars


        GetComponent<Collider>().enabled = false;
        anim.SetTrigger("death");

    }

    // TODO sort voids


    #region IInitialState implementation

    public void FixateState() { }
    public void ReturnToInitialTurnState()
    {
        transform.position = player.turn.actions_Downside[0].Point; // definitely MoveAct. Def-ly with init pos
        LookAt( player.turn.actions_Upside[0].Point ); // and def-y LookAct
    }

    #endregion


}

[Serializable]
public class MoveAction : TurnAction, Down
{
    [NonSerialized]
    // for not long way to avatar (thru turn.Owner.av..)
    PlayerAvatar avatar;
    [NonSerialized]
    float distanceDelta = 0f;

    float Timer = 0;

    public MoveAction(Turn turn)
    {
        this.turn = turn;
        avatar = turn.Owner.avatar;

        Point = avatar.transform.position;
    }
    
    public override void InputHandler()
    {
        Vector3 moveToward = new Vector3(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical"));

        if (moveToward.magnitude > float.Epsilon)
        {
            avatar.Move(moveToward);

            if (distanceDelta < 0.22f) 
                distanceDelta += moveToward.magnitude * avatar.speed; 
            else
            {
                // Dump moveAct, add new one and relink inpHandler
                Confirmed = true;
                ChangeStateTo_NewMoveAct();
                
            }
            return;
        }
        else
        {
            //avatar.anim.SetFloat("Velocity", 0); // temp in here
        }

        Timer += Time.deltaTime;
    }

    void ChangeStateTo_NewMoveAct()
    {
        turn.actions_Downside.Add(new MoveAction(turn));
        turn.Owner.SetInputHandlerOnLast_Down();
    }

    public override bool Action()
    {
        Vector3 moveTo =
            Point - avatar.transform.position;

        if(moveTo.magnitude > 0.02f)
            avatar.Move(moveTo.normalized);

        Timer -= Time.deltaTime;
        return moveTo.magnitude <= 0.1f
            && Timer <= 0; 
    }

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        avatar = turn.Owner.avatar;
    }

}


[Serializable]
public class LookArountAction : TurnAction, Up, IUsingFloorCursor
{
    [NonSerialized]
    WeaponHandler weaponHandler;
    [NonSerialized]
    PlayerAvatar avatar;
    [NonSerialized]
    float dif = 0;

    float Timer = 0;

    public LookArountAction(Turn turn, Vector3 point)
    {
        SyncNonSync(turn);

        Point = point;
        Confirmed = true;

        ActivateFCursor();
    }


    public override void InputHandler()
    {
        // TODO check for availabilty 
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Floor")
        {
            float anlge = Vector3.Angle(
                            avatar.transform.forward,
                            hit.point - avatar.transform.position
                          );
            dif += Mathf.Abs(anlge);

            avatar.LookAtSmooth(hit.point);
            ControllingFCursor(hit.point);

            if (dif > 24) // Dump  // 24• seems small, but enough
            {
                //Confirmed = true; DEFAULT state of Confirmed
                ChangeStateTo_NewLookAction(hit.point);
                return;
            }


            if (Input.GetMouseButtonDown(0))
            {
                // TODO spawn new cursors
                ChangeFCursotState(GameManager.UI.FloorCursorState.Setted);

                turn.actions_Upside.Add(new LookArountAction(turn, hit.point)); // Dump
                ChangeStateTo_WeaponAct(hit.point);

                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            turn.actions_Upside.Add(new LookArountAction(turn, hit.point)); // Dump
            ChangeStateTo_Reload(hit.point);
        }

        Timer += Time.deltaTime;
    }

    IEnumerator WaitAnd(Action whatNext, float awaitTime)
    {
        yield return new WaitForSeconds(awaitTime);
        whatNext();
    }

    void ChangeStateTo_NewLookAction(Vector3 point)
    {
        //DisableFCursor();
        turn.actions_Upside.Add(new LookArountAction(turn, point));
        turn.Owner.SetInputHandlerOnLast_Up();
    }

    void ChangeStateTo_WeaponAct(Vector3 point)
    {
        turn.actions_Upside.Add(new WeaponAction(turn, point));
        turn.Owner.SetInputHandlerOnLast_Up();
    }

    void ChangeStateTo_Reload(Vector3 point)
    {
        turn.actions_Upside.Add(new WeaponAction_Reload(turn, point));
        turn.Owner.SetInputHandlerOnLast_Up();
    }


    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        weaponHandler = turn.Owner.loadout.WeaponHandler;
        avatar = turn.Owner.avatar;
    }

    public override bool Action()
    {
        avatar.LookAtSmooth(Point);

        Timer -= Time.deltaTime;
        return Timer <= 0;
    }



    #region IFloorCursot implementation

    public void ActivateFCursor()
    {
        if (turn.Performing) return;

        turn.Owner.ui.FloorCursorAnch.localScale
            = new Vector3(weaponHandler.info.Radius * 2, 1, weaponHandler.info.Radius * 2);
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

[Serializable]
public class WeaponAction : TurnAction, Up, IUsingFloorCursor
{
    [NonSerialized]
    WeaponHandler weaponHandler;
    [NonSerialized]
    bool can = true;

    public WeaponAction(Turn turn, Vector3 point)
    {
        Point = point;

    // TODO same call \/ at old realized Actions
        SyncNonSync(turn);

        ActivateFCursor();
    }


    public override void InputHandler()
    {
        if (!can) return;

        // TODO check for availabilty 
        // TODO GetSuccessQ
        // TODO shot even if the mag is empty?                

        Confirmed = true;
        
        can = false; // TODO commonize (forgot that word)

        weaponHandler.Shoot(this, OnShotedCallback);
    }

    public void OnShotedCallback()
    {
        ChangeStateTo_LookAct();
    }
    

    void ChangeStateTo_LookAct()
    {
        turn.actions_Upside.Add(new LookArountAction(turn, Point));
        turn.Owner.SetInputHandlerOnLast_Up();
    }
    

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        weaponHandler = turn.Owner.loadout.WeaponHandler;
        //avatar = turn.Owner.avatar;
    }

    public override bool Action()
    {
        if (!ActionStarted)
            weaponHandler.Shoot(this);

        return ActionEnded;
    }



    #region IFloorCursot implementation

    public void ActivateFCursor()
    {
        if (turn.Performing) return;

        turn.Owner.ui.FloorCursorAnch.localScale
            = new Vector3(weaponHandler.info.Radius*2, 1, weaponHandler.info.Radius * 2);
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

[Serializable]
public class WeaponAction_Reload : TurnAction, Up
{
    [NonSerialized]
    WeaponHandler weaponHandler;

    [NonSerialized]
    bool can = true;

    public WeaponAction_Reload(Turn turn, Vector3 point)
    {
        Point = point;
        SyncNonSync(turn);
    }

    public override void InputHandler()
    {
        // + measure success Q, that will be affect on time of reload TODO
        if (!can) return;

        weaponHandler.Reload(this, OnReloadedCallback);
        can = false;
    }

    public void OnReloadedCallback()
    {
        Confirmed = true;
        ChangeStateTo_LookAct();
    }

    void ChangeStateTo_LookAct()
    {
        turn.actions_Upside.Add(new LookArountAction(turn, Point));
        turn.Owner.SetInputHandlerOnLast_Up();
    }

    public override bool Action()
    {
        if (!ActionStarted)
            weaponHandler.Reload(this);

        return ActionEnded; 
    }

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        weaponHandler = turn.Owner.loadout.WeaponHandler;
    }

}