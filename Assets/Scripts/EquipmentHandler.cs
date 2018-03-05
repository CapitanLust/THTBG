using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentHandler : MonoBehaviour
{
    public bool ActionStarted = false,
                ActionEnded   = false;
}

public class WeaponHandler : EquipmentHandler
{
    public Weapon info;

    public int CurMag;

    public Animator anim;
    public Transform bulletPrefab, 
                     bulletSpawn;
    
    public void Init(Weapon info)
    {
        this.info = info;
        CurMag = info.Mag;
    }

    public virtual void Shoot(Vector3 point)
    {
        ActionStarted = true;
        // (!!!) Temporary on WaitForSeconds
        // it will be on Animator in the future        
        VisualShot(point);
        StartCoroutine(WaitAnd(
            () => {
                ProcessShoot(point);
            },
            .5f
        ));
            
    }

    protected virtual void ProcessShoot(Vector3 point)
    {
        if (CurMag > 0)
        {
            CurMag--;
            MeasureAftermath(point);
        }

        ActionEnded = true;
    }
    public virtual void MeasureAftermath(Vector3 point)
    {

    }

    public virtual void Reload() 
    {
        ActionStarted = true;        
        StartCoroutine(WaitAnd(
            () => {
                if (CurMag != 0)
                    CurMag = info.Mag + 1;
                else
                    CurMag = info.Mag;

                ActionEnded = true;
            },
            info.ReloadTime
        ));
    }

    IEnumerator WaitAnd (Action whatNext, float awaitTime)
    {
        yield return new WaitForSeconds(awaitTime);
        whatNext();
    }


    // TEMP:
    public void VisualShot(Vector3 point)
    {
        anim.SetTrigger("Shoot");

        var newBull = Instantiate<Transform>(bulletPrefab, bulletSpawn);
        //newBull.Rotate() -- random spread or aimed to the victim avatar
    }

}