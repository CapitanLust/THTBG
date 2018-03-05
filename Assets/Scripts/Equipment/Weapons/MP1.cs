using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MP1 : WeaponHandler {
    
    public override void Shoot(Vector3 point)
    {
        base.Shoot(point);
        Debug.Log("MP1!!");
    }

    public override void MeasureAftermath(Vector3 point)
    {
        Debug.Log("measuring shot for MP1");
        base.MeasureAftermath(point);
    }

}
