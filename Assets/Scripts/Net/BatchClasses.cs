using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;



[Serializable]
public class Turn
{
    /*
    public List<TurnAction> actions
        = new List<TurnAction>();*/
    public List<TurnAction> actions_Upside   = new List<TurnAction>(),
                            actions_Downside = new List<TurnAction>();

    [NonSerialized]
    public Player Owner;

    [NonSerialized]
    public int actionsDid_Up = 0;
    [NonSerialized]
    public int actionsDid_Down = 0;
    [NonSerialized]
    public bool Performing = false;

    [Obsolete]
    public void Perform() { }

    public void Clear()
    {
        actions_Upside.Clear();
        actions_Downside.Clear();
        actionsDid_Up = 0;
        actionsDid_Down = 0;
    }

    public void ClearOfUnconfirmed()
    {
        actions_Upside = actions_Upside.FindAll(x => x.Confirmed);
        actions_Downside = actions_Downside.FindAll(x => x.Confirmed);
    }

    public bool Iterate()
    {
        if ( actionsDid_Up   >= actions_Upside.Count &&
             actionsDid_Down >= actions_Downside.Count )
                return true;

        if ( actionsDid_Up<actions_Upside.Count && 
             actions_Upside[actionsDid_Up].Action() )
                actionsDid_Up++;

        if ( actionsDid_Down<actions_Downside.Count &&
             actions_Downside[actionsDid_Down].Action() )
                actionsDid_Down++;

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


//  No functional facilities.
//  Just for more formal distinguish of TurnAction
public interface Up { }
public interface Down { }

public interface IUsingFloorCursor
{
    void ActivateFCursor();
    void ControllingFCursor(Vector3 point);
    void ChangeFCursotState(GameManager.UI.FloorCursorState state);
    void DisableFCursor();
}