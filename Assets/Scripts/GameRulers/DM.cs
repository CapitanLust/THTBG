using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DM : GameRuler {

    bool firstSpawn = false;

    public override bool CanRevive()
    {
        if(!firstSpawn)
            return firstSpawn = true;
        return false;
    }

    public override bool CheckMatchForEnd()
    {
        int survived = 0;
        foreach (var p in gameManager.players)
            if (p.isAlive) survived++;
        return survived == 1;
    }
    
}
