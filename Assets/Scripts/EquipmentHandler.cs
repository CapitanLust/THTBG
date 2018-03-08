using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentHandler : MonoBehaviour
{
    //public bool ActionStarted = false,
                //ActionEnded   = false;
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
    public virtual void Shoot(TurnAction tAction /*Vector3 point, float successQ*/)
    {
        tAction.ActionStarted = true;
        // (!!!) Temporary on WaitForSeconds
        // it will be on Animator in the future        
        VisualShot(tAction.Point);
        StartCoroutine(WaitAnd(
            () => {
                ProcessShoot(tAction);
            },
            .5f
        ));
            
    }

    protected virtual void ProcessShoot(TurnAction tAction)
    {
        if (CurMag > 0)
        {
            CurMag--;
            if(player.hasAuthority)
                MeasureAftermath(tAction);
        }

        tAction.ActionEnded = true;
    }
    public virtual void MeasureAftermath(TurnAction tAction)
    {
        foreach(var p in player.gameManager.players)
        {
            // TODO foreach avatar
            if (!p.isAlive) continue;

            // and TODO mask for breakable-thru walls
            var difVector = p.avatar.transform.position - tAction.Point;
            Debug.Log("[");
            Debug.Log(p.avatar.transform.position);
            Debug.Log(tAction.Point);
            Debug.Log(difVector.magnitude);
            Debug.Log(info.Radius + p.avatar.Radius);
            Debug.Log("]");
            if (difVector.magnitude > info.Radius + p.avatar.Radius)
                continue;

            var ray = new Ray(tAction.Point, p.avatar.transform.position - tAction.Point);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Player Avatar")
                SendDamage(hit.collider.GetComponent<PlayerAvatar>(), tAction.SuccesQuotient);
            else
                Debug.Log("ne");
        }
    }

    public void SendDamage(PlayerAvatar victim, float successQ)
    {
        victim.CmdGetDamage(info.Damage * successQ, player.Nik, successQ);
        Debug.Log(player.Nik + " Hitted " + victim.player.Nik);
        // TODO hitmarker
        player.ui.Hitmarker();
    }

    public virtual void Reload(TurnAction tAction) 
    {
        tAction.ActionStarted = true;        
        StartCoroutine(WaitAnd(
            () => {
                if (CurMag != 0)
                    CurMag = info.Mag + 1;
                else
                    CurMag = info.Mag;

                tAction.ActionEnded = true;
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
        if (CurMag > 0)
        {
            anim.SetTrigger("Shoot");

            Instantiate<Transform>(bulletPrefab, bulletSpawn);
            //newBull.Rotate() -- random spread or aimed to the victim avatar
        }
    }

}