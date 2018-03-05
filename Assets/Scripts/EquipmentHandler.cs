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

    public Player player;
    
    public void Init(Weapon info, Player player)
    {
        this.info = info;
        CurMag = info.Mag;

        this.player = player;
    }

    // TODO change arguments to link to Turn
    public virtual void Shoot(Vector3 point, float successQ)
    {
        ActionStarted = true;
        // (!!!) Temporary on WaitForSeconds
        // it will be on Animator in the future        
        VisualShot(point);
        StartCoroutine(WaitAnd(
            () => {
                ProcessShoot(point, successQ);
            },
            .5f
        ));
            
    }

    protected virtual void ProcessShoot(Vector3 point, float successQ)
    {
        if (CurMag > 0)
        {
            CurMag--;
            if(player.hasAuthority)
                MeasureAftermath(point, successQ);
        }

        ActionEnded = true;
    }
    public virtual void MeasureAftermath(Vector3 point, float successQ)
    {
        foreach(var p in player.gameManager.players)
        {
            // ray for check walls
            // and TODO mask for breakable-thru walls
            var difVector = p.avatar.transform.position - point;
            if (difVector.magnitude >= info.Radius + p.avatar.Radius)
                continue;

            var ray = new Ray(point, p.avatar.transform.position - point);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit) && hit.collider.tag == "Player Avatar")
                SendDamage( hit.collider.GetComponent<PlayerAvatar>(), successQ );
        }
    }

    public void SendDamage(PlayerAvatar victim, float successQ)
    {
        victim.CmdGetDamage(info.Damage * successQ, player.Nik, successQ);
        Debug.Log(player.Nik + " Hitted " + victim.player.Nik);
        // TODO hitmarker
        player.ui.Hitmarker();
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

        Instantiate<Transform>(bulletPrefab, bulletSpawn);
        //newBull.Rotate() -- random spread or aimed to the victim avatar
    }

}