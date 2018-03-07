using System;
using System.Collections.Generic;
using UnityEngine;

public class Grach : WeaponHandler {
    
    public override void Shoot(TurnAction tAction)
    {
        base.Shoot(tAction);
        Debug.Log("Gra4!!");        
    }

}

