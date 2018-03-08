using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;


    
[Obsolete]
[Serializable]
public class Batch
{
    /*
    public byte[] Serialized
    {
        get
        {
            return Cmn.SerializeBatch(this);
        }
    }
    public static Batch Deserialize (byte[] barray)
    {
        return Cmn.DeserializeTurn(barray);
    }
    */
    //public string PlayerOwnerNik;

    [NonSerialized]
    public Player Owner;

    // it was designed for abstract. But we can't use abst with u.networking
    public virtual void Perform() {} // TODO remove from here.
}

[Serializable]
public class Turn
{
    // private logic but 'public'
    public List<TurnAction> actions
        = new List<TurnAction>();

    [NonSerialized]
    public Player Owner;

    [NonSerialized]
    public int actionsDid = 0;
    [NonSerialized]
    public bool Performing = false;

    public void Perform()
    {
        //if (Iterate() || actionsDid >= actions.Count)
                      // /\ second check because of -- if we complete it for 1 step.
                     // ( solution designed for performing through Update and frame-logic )
            //Owner.CmdSetReady();
        // /\ complitely moved to update
    }

    public void Clear()
    {
        actions.Clear();
        actionsDid = 0; 
    }

    public void ClearOfUnconfirmed()
    {
        actions = actions.FindAll(x => x.Confirmed);
    }

    public bool Iterate()
    {
        if (actionsDid >= actions.Count) return true;

        if (actions[actionsDid].Action())
            actionsDid++;

        return false;
    }

    #region sync-ing

    public byte[] Serialized
    {
        get
        {
            /// !!! IMPORTANT !!
            ClearOfUnconfirmed();
            return Cmn.SerializeTurn(this);
        }
    }
    public static Turn Deserialize(byte[] barray)
    {
        return Cmn.DeserializeTurn(barray);
    }


    #endregion

}


[Serializable]
public abstract class TurnAction
{
    [NonSerialized]
    public Turn turn;

    [NonSerialized]
    public bool Confirmed = false;
    [NonSerialized]
    public bool ActionStarted = false;
    [NonSerialized]
    public bool ActionEnded = false;
    

    float x = 0, y = 0, z = 0;
    public Vector3 Point
    {
        get { return new Vector3(x, y, z); }
        set { x = value.x; y = value.y; z = value.z; }
    }

    public float SuccesQuotient = 1;

    [NonSerialized]
    public float Radius;

    // TODO have own FloorCursor


    public abstract void InputHandler();

    /// <returns> has action complete? </returns>
    public abstract bool Action();

    /// <summary>
    /// Uses for reapply NonSync unique fields after sync
    /// (like avatar, etc)
    /// </summary>
    public abstract void SyncNonSync(Turn turn);

}


public interface IUsingFloorCursor
{
    void ActivateFCursor();
    void ControllingFCursor(Vector3 point);
    void ChangeFCursotState(GameManager.UI.FloorCursorState state);
    void DisableFCursor();
}