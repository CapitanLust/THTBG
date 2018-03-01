using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAvatar : MonoBehaviour {

    public Player player;

    public float speed = 0.1f;
    public bool isTurning = false,
                isCommonTurning = false;

    //public Turn turn = new Turn();

    // cosmetic // TODO incapsule
    //public Material 

    public void update() // notice that there's lowercase 'u'
    {               // bcse call goes from Player
        IterateTurn();
    }

    public void Move (Vector3 toward)
    {
        transform.position += toward * speed;
        //LookAtDir(toward);
        LookAtDirSmooth(toward);
    }

    public void Combat (Vector3 point)
    {
        Debug.Log("Combat at: " + point);
    }

    // User Part

    //float distanceDelta = 0;
    Vector3 initialTurnPos;
    
    public void StartTurning()
    {

        initialTurnPos = transform.position;
        //distanceDelta = 0;
        //turn.Clear();

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

    public void ReturnToInitialTurnState()
    {
        Debug.Log("asd" + player.turn.actions[0].Point);
        //transform.position = initialTurnPos;
        transform.position = player.turn.actions[0].Point; // definitely MoveAct. Def-ly with init pos
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


    // TODO sort voids

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

            if (distanceDelta < 0.2f) 
                distanceDelta += moveToward.magnitude * avatar.speed; 
            else
            {
                //turn.actions.Add(new Move() { point = transform.position });

                // Dump moveAct, add new one and relink inpHandler
                //Point = avatar.transform.position;
                turn.actions.Add(new MoveAction(turn));
                turn.Owner.inputHandler = turn.actions[turn.actions.Count-1].InputHandler;

                Debug.Log("added move");
            }
            return;
        }



        // if input type changed, switch to new action TODO
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

/*
public class Combat : TurnAction
{
    [NonSerialized]
    bool setted = false;

    public override bool Action()
    {
        // avatar.Weapon (field)
        //avatar.Combat(Point);
        Debug.Log("wtfc");
        return true;
    }
}*/