using System;
using System.Collections.Generic;
using UnityEngine;

public class Grach : WeaponHandler {
    
    public override void Shoot(Vector3 point)
    {
        base.Shoot(point);
        Debug.Log("Gra4!!");        
    }

}

