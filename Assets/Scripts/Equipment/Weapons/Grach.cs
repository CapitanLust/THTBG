using System;
using System.Collections.Generic;
using UnityEngine;

public class Grach : WeaponHandler {
    
    public override void Shoot(Vector3 point, float successQ)
    {
        base.Shoot(point, successQ);
        Debug.Log("Gra4!!");        
    }

}

