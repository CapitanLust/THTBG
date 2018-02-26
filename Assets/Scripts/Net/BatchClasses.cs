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

    public int actionsDid = 0;

    public void Perform()
    {
        Debug.Log("a");
        if (Iterate() || actionsDid >= actions.Count)
        {
          //Clear();  // /\ second check because of -- if we complete it for 1 step.
                      // ( solution designed for performing through Update and frame-logic )
            Debug.Log("b");
            Owner.CmdSetReady();
        }
    }

    public void Clear()
    {
        actions.Clear();
        actionsDid = 0; // TODO ?

    }

    public bool Iterate()
    {
        if (actionsDid >= actions.Count) return true;

        if (actions[actionsDid].Action())
        {
            actionsDid++;
            Debug.Log("c");
        }
        Debug.Log("d");

        return false;
    }

    #region sync-ing

    public byte[] Serialized
    {
        get
        {
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
    public Turn turn;

    float x = 0, y = 0, z = 0;
    public Vector3 Point
    {
        get { return new Vector3(x, y, z); }
        set { x = value.x; y = value.y; z = value.z; }
    }

    public float SuccesQuotient;

    public abstract void InputHandler();

    /// <returns> has action complete? </returns>
    public abstract bool Action();

}


