using System.Collections;
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

    float distanceDelta = 0;
    Vector3 initialTurnPos;
    
    public void StartTurning()
    {

        initialTurnPos = transform.position;
        distanceDelta = 0;
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

public class Move : TurnAction
{
    public override bool Action()
    {
        //Vector3 moveTo = point - avatar.transform.position;
        //avatar.Move(moveTo.normalized);

        //return moveTo.magnitude <= 0.1f; // Or Epsilon?
        Debug.Log("wtfm");
        return true;
    }
}

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
