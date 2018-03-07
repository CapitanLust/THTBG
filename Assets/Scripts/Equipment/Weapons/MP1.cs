using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MP1 : WeaponHandler {
    
    public override void Shoot(TurnAction tAction)
    {
        base.Shoot(tAction);
        Debug.Log("MP1!!");
    }

    public override void MeasureAftermath(TurnAction tAction)
    {
        Debug.Log("measuring shot for MP1");
        base.MeasureAftermath(tAction);
    }

}
