using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MP1 : WeaponHandler {
    
    public override void Shoot(Vector3 point, float successQ)
    {
        base.Shoot(point, successQ);
        Debug.Log("MP1!!");
    }

    public override void MeasureAftermath(Vector3 point, float successQ)
    {
        Debug.Log("measuring shot for MP1");
        base.MeasureAftermath(point, successQ);
    }

}
