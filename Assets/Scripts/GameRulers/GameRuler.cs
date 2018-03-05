using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]                  // MonoBehaviour to attach as modul. 
                                // Strange idea, but so emplemented in Unity's Tanks
public abstract class GameRuler : MonoBehaviour {

    public bool FriendlyFire = false;


    public GameManager gameManager;

    public virtual bool CheckMatchForEnd()
    {
        return false;
    }

    public virtual bool IsObjectVisible()
    {
        return true;
    }

    public virtual bool CanRevive() { return false; }

}
