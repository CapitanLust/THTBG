using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAvatar : MonoBehaviour {

    public Player player;

    public float speed = 0.1f;
    public bool isTurning = false,
                isCommonTurning = false;

    public Turn turn = new Turn();

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
        turn.Clear();

        isCommonTurning = false;
        isTurning = true;
    }

    public void IterateTurn()
    {
        if (turn.Iterate(this))
        {
            isCommonTurning = false;
            //player.CmdSetReady();
            Debug.Log("Ended");
        }
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
            turn.actions.Add(new Move() { point = transform.position });

            isTurning = false;
            //player.isReady = true; // also calls hook
            //player.gameManager.Ready("as");
            //player.CmdSetReady(); // TODO !!! Cmd from hook. And just '=' there

            ResetToInitial();

            //isCommonTurning = true;

            return;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ResetToInitial();
            turn.Clear();
            return;
        }

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

    }


    public void StartCommonTurning()
    {
        isTurning = false;
        isCommonTurning = true;
    }



}


public class Turn // TODO derivate batch
{
    // private logic but 'public'
    public List<TurnAction> actions
        = new List<TurnAction>();

    public int actionsDid = 0;

    public void Clear()
    {
        actions.Clear();
    }

    public bool Iterate(PlayerAvatar avatar) // TODO change linking logic there
    {
        if (actionsDid >= actions.Count) return true;

        if (actions[actionsDid].Action(avatar))
            actionsDid++;

        //Iterate(avatar); ::: Forgot about upd physics
        return false;
    }
}

public abstract class TurnAction
{
    public Vector3 point;
    //public float succesQuotient;

    /// <returns> has action complete? </returns>
    public abstract bool Action(PlayerAvatar avatar);
}

public class Move : TurnAction
{
    public override bool Action(PlayerAvatar avatar)
    {
        Vector3 moveTo = point - avatar.transform.position;
        avatar.Move(moveTo.normalized);

        return moveTo.magnitude <= 0.1f; // Or Epsilon?
    }
}

public class Combat : TurnAction
{
    public override bool Action(PlayerAvatar avatar)
    {
        // avatar.Weapon (field)
        avatar.Combat(point);
        return true;
    }
}
