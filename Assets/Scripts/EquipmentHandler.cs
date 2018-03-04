using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentHandler : MonoBehaviour {}

public class WeaponHandler : EquipmentHandler
{
    public Weapon info;

    public int CurMag;

    public bool ShootedOne = false,
                StartedShoot = false;

    public void Init(Weapon info)
    {
        this.info = info;
        CurMag = info.Mag;
    }

    public virtual void Shoot() // TODO or shot?
    {
        StartedShoot = true;
        // (!!!) Temporary on WaitForSeconds
        // it will be on Animator in the future
        StartCoroutine(WaitAnd(
            () => {
                CurMag--;
                ShootedOne = true;
            },
            2
        ));
    }

    IEnumerator WaitAnd (Action whatNext, float awaitTime)
    {
        yield return new WaitForSeconds(awaitTime);
        whatNext();
    }

}