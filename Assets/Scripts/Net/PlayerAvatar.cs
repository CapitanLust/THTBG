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
        if (isTurning)
        {
            InputTurn();
        }
        else if (isCommonTurning)
        {
            IterateTurn();
        }
    }

    public void Move (Vector3 toward)
    {
        transform.position += toward * speed;
        //transform.position += new Vector3(toward.x * speed, 0, toward.y * speed);
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
        //if (turn.Iterate())
        //{
            //isCommonTurning = false;
            //player.CmdSetReady();
            //Debug.Log("Ended");
        //}
    }

    public void ResetToInitial()
    {
        transform.position = initialTurnPos;
    }

    public void InputTurn ()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            // last position ~confirmation~
            //turn.actions.Add(new Move() { Point = transform.position });

            isTurning = false;

            ResetToInitial();

            //isCommonTurning = true;

            return;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Cancel();
            return;
        }

        /*
        Vector3 moveToward = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );
        if( moveToward.magnitude > float.Epsilon)
        {
            Move(moveToward);

            if (distanceDelta < 0.2f) { distanceDelta += moveToward.magnitude*speed; } // TODO *speed?
            else
            {
                turn.actions.Add( new Move() { point = transform.position } );
                distanceDelta = 0;

                Debug.Log("added move");
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            turn.actions.Add(new Combat());
        }
        */

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



}

[Serializable]
public class MoveAction : TurnAction
{
    [NonSerialized]
    // for not long way to avatar (thru turn.Owner.av..)
    PlayerAvatar avatar;
    [NonSerialized]
    float distanceDelta = 0;

    public MoveAction(Turn turn)
    {
        this.turn = turn;
        avatar = turn.Owner.avatar;
    }

    public override void InputHandler()
    {
        Vector3 moveToward = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
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
                Point = avatar.transform.position;
                turn.actions.Add(new MoveAction(turn));
                turn.Owner.inputHandler = turn.actions[0].InputHandler;

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
}

/*
public class Combat : TurnAction
{
    public override bool Action()
    {
        // avatar.Weapon (field)
        //avatar.Combat(Point);
        Debug.Log("wtfc");
        return true;
    }
}
*/