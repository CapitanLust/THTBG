using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]                  // MonoBehaviour to attach as modul. 
                                // Strange idea, but so emplemented in Unity's Tanks
public abstract class GameRuler : MonoBehaviour {

    #region Linking var-s
    public GameManager gameManager;
    #endregion

    public bool FriendlyFire = false;

    public virtual int CountOfPendingPlayers
    {
        get
        {
            int c = 0;
            foreach (var p in gameManager.players)
                if (!p.isDead) c++;
            return c;
        }
    }

    public virtual bool CheckMatchForEnd()
    {
        return false;
    }

    public virtual bool IsObjectVisible()
    {
        return true;
    }

    public virtual bool CanRevive(Player p) { return false; }

}
