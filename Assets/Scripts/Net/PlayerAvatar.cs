﻿using System;
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

    public void update() // notice that there's lowercase 'u'
    {               // bcse call goes from Player
        IterateTurn();
    }

    public void Update()
    { 
        SyncWithAnimator();
    }

    public void SyncWithAnimator()
    {
        //anim.SetFloat("velocity", rg.velocity.magnitude);
    }

    public void Move (Vector3 toward)
    {
        transform.position += toward * speed;
        anim.SetFloat("velocity", speed);

        //LookAtDir(toward);
        LookAtDirSmooth(toward);
    }

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


    public void LookAt (Vector3 point)
    {
        var difNorm = (point - transform.position).normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 90f - angle, 0f);
        // don't call other method to little speed up
    }
    public void LookAtDir (Vector3 direction)
    {
        var difNorm = direction.normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 90f - angle, 0f);
    }

    public void LookAtDirSmooth (Vector3 direction)
    {
        var difNorm = direction.normalized;
        float angle = Mathf.Atan2(difNorm.z, difNorm.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.Euler(0f, 90f - angle, 0f),
                .427f // (that accuracy after 0.4 is just random approximation from my head)
                // i think, u know that feelin' :D
            );
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
        player.gameManager.ui.UpdPlayerList(player.gameManager.players);

        if (HP <= 0)
        {
            RpcDie();
            player.gameManager.CmdAwardKiller(hitterNik, successQ);
        }
    }
    
    [ClientRpc]
    public void RpcDie()
    {
        player.update = player.Update_Empty;
        player.isAlive = false;
        player.isDead = true;
        player.avatar = null;
        //Remove from avatars

        player.CmdSetReady();

        GetComponent<Collider>().enabled = false;
        anim.SetTrigger("death");

    }

    // TODO sort voids


    #region IInitialState implementation

    public void FixateState() { }
    public void ReturnToInitialTurnState()
    {
        transform.position = player.turn.actions[0].Point; // definitely MoveAct. Def-ly with init pos
    }

    #endregion


}

[Serializable]
public class MoveAction : TurnAction
{
    [NonSerialized]
    // for not long way to avatar (thru turn.Owner.av..)
    PlayerAvatar avatar;
    [NonSerialized]
    float distanceDelta = 0f;

    public MoveAction(Turn turn)
    {
        this.turn = turn;
        avatar = turn.Owner.avatar;
        oldMousePos = Input.mousePosition;

        Point = avatar.transform.position;
    }

    [NonSerialized]
    Vector3 oldMousePos;
    public override void InputHandler()
    {
        Vector3 moveToward = new Vector3(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical"));

        if (moveToward.magnitude > float.Epsilon)
        {
            avatar.Move(moveToward);

            if (distanceDelta < 0.14f) 
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
            avatar.anim.SetFloat("velocity", 0); // temp in here

            if((Input.mousePosition - oldMousePos).magnitude > .002f)
            {
                Confirmed = true;
                ChangeStateTo_WeaponAct();
            }
        }


        oldMousePos = Input.mousePosition;
    }

    void ChangeStateTo_NewMoveAct()
    {
        turn.actions.Add(new MoveAction(turn));
        turn.Owner.SetInputHandlerOnLast();
    }
    void ChangeStateTo_WeaponAct()
    {
        turn.actions.Add(new MoveAction(turn));
        turn.actions.Add(new WeaponAction(turn));
        turn.Owner.SetInputHandlerOnLast();
    }

    public override bool Action()
    {
        Vector3 moveTo =
            Point - avatar.transform.position;
        avatar.Move(moveTo.normalized);

        return moveTo.magnitude <= 0.1f; // Or Epsilon?
    }

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        avatar = turn.Owner.avatar;
    }

}


[Serializable]
public class WeaponAction : TurnAction, IUsingFloorCursor
{
    [NonSerialized]
    WeaponHandler weaponHandler;
    [NonSerialized]
    PlayerAvatar avatar;
    [NonSerialized]
    bool can = true;

    public WeaponAction(Turn turn)
    {
    // TODO same call \/ at old realized Actions
        SyncNonSync(turn);

        ActivateFCursor();
    }


    public override void InputHandler()
    {
        if (!can) return;

        // TODO check for availabilty 
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Floor")
        {
            avatar.LookAt(hit.point);

            ControllingFCursor(hit.point);

            if (Input.GetMouseButtonDown(0))
            {
                // TODO GetSuccessQ

                // TODO shot even if the mag is empty?

                Point = hit.point;

                // TODO spawn new cursors
                ChangeFCursotState(GameManager.UI.FloorCursorState.Setted);

                Confirmed = true;

                // temp wait TODO
                can = false; // TODO commonize (forgot that word)

                weaponHandler.Shoot(this);
                weaponHandler.StartCoroutine(WaitAnd(() => 
                    {
                        ChangeStateTo_WeaponAct();
                    },
                    .5f
                ));

                return;
            }
        }
        
        if (Mathf.Abs( Input.GetAxis("Horizontal")) > float.Epsilon
            || Mathf.Abs(Input.GetAxis("Vertical")) > float.Epsilon)
            ChangeStateTo_MoveAction();

        else if(Input.GetKeyDown(KeyCode.R))
            ChangeStateTo_Reload();
    }
    IEnumerator WaitAnd(Action whatNext, float awaitTime)
    {
        yield return new WaitForSeconds(awaitTime);
        whatNext();
    }

    void ChangeStateTo_MoveAction()
    {
        DisableFCursor();
        turn.actions.Add(new MoveAction(turn));
        turn.Owner.SetInputHandlerOnLast();
    }

    void ChangeStateTo_WeaponAct()
    {
        turn.actions.Add(new WeaponAction(turn));
        turn.Owner.SetInputHandlerOnLast();
    }

    void ChangeStateTo_Reload()
    {
        turn.actions.Add(new WeaponAction_Reload(turn));
        turn.Owner.SetInputHandlerOnLast();
    }


    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        weaponHandler = turn.Owner.loadout.WeaponHandler;
        avatar = turn.Owner.avatar;
    }

    public override bool Action()
    {
        avatar.LookAt(Point);

        // TODO review this logic
        if (!ActionStarted)
            weaponHandler.Shoot(this);
        //if (ActionEnded)
        //{
            //ActionStarted = false;
            //ActionEnded = false;
            //return true;
        //}
        return ActionEnded;
        //return false;
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
public class WeaponAction_Reload : TurnAction
{
    [NonSerialized]
    WeaponHandler weaponHandler;

    public WeaponAction_Reload(Turn turn)
    {
        SyncNonSync(turn);
    }

    public override void InputHandler()
    {
        // + measure success Q, that will be affect on time of reload TODO

        // temp:
        weaponHandler.Reload(this);
        weaponHandler.StartCoroutine(WaitAnd(
            () => {
                Confirmed = true;
                ChangeStateTo_WeaponAct();
            },
            weaponHandler.info.ReloadTime
        ));
    }
    IEnumerator WaitAnd(Action whatNext, float awaitTime)
    {
        yield return new WaitForSeconds(awaitTime);
        whatNext();
    }

    void ChangeStateTo_WeaponAct()
    {
        turn.actions.Add(new WeaponAction(turn));
        turn.Owner.SetInputHandlerOnLast();
    }

    public override bool Action()
    {
        // TODO review this logic
        if (!ActionStarted)
            weaponHandler.Reload(this);
        //if (ActionEnded)
        //{                                 no need after true
        //    ActionStarted = false;
        //    ActionEnded = false;
        //    return true;
        //}

        return ActionEnded; 
    }

    public override void SyncNonSync(Turn turn)
    {
        this.turn = turn;
        weaponHandler = turn.Owner.loadout.WeaponHandler;
    }

}