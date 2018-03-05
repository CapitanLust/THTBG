using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DM : GameRuler {

    public override bool CheckMatchForEnd()
    {
        int survived = 0;
        foreach (var p in gameManager.players)
            if (p.isAlive) survived++;
        return survived == 1;
    }

}
