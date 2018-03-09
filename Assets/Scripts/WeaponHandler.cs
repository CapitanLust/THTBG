using System;
using System.Collections;
using UnityEngine;

public class WeaponHandler : EquipmentHandler
{
    public Weapon info;

    public int curMag;
    public int CurMag
    {
        get { return curMag; }
        set
        {
            curMag = value;

            if(player.hasAuthority) // TODO check performance
                player.ui.UpdateWeaponPanel_Ammo(this);
        }
    }

    public Animator anim;
    public Transform bulletPrefab,
                     bulletSpawn;

    public Player player;

    public void Init(Weapon info, Player player)
    {
        this.player = player;

        this.info = info;
        CurMag = info.Mag;
    }

    // TODO change arguments to link to Turn
    public virtual void Shoot(TurnAction tAction /*Vector3 point, float successQ*/)
    {
        tAction.ActionStarted = true;
        // (!!!) Temporary on WaitForSeconds
        // it will be on Animator in the future

        if (CurMag > 0)
        {
            VisualShot(tAction.Point);
            CurMag--;

            if (tAction.turn.Performing)
                StartCoroutine(WaitAnd(() =>
                {
                    ProcessShoot(tAction);
                },
                    .5f
                ));
        }
        else
            StartCoroutine(WaitAnd(() => {
                tAction.ActionEnded = true;
            }, .4f));
        // TODO else *click*

    }

    protected virtual void ProcessShoot(TurnAction tAction)
    {
        if (player.hasAuthority)
            MeasureAftermath(tAction);

        tAction.ActionEnded = true;
    }
    public virtual void MeasureAftermath(TurnAction tAction)
    {
        foreach (var p in player.gameManager.players)
        {
            // TODO foreach avatar
            // and (TODO) with ruler
            if (!p.isAlive) continue;

            var difVector = p.avatar.transform.position - tAction.Point;
            if (difVector.magnitude > info.Radius + p.avatar.Radius)
                continue;

            // and TODO mask for breakable-thru walls
            var ray = new Ray(tAction.Point, p.avatar.transform.position - tAction.Point);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Player Avatar")
                SendDamage(hit.collider.GetComponent<PlayerAvatar>(), tAction.SuccesQuotient);
            else
                Debug.Log("ne " + hit.collider.name);
        }
    }

    public void SendDamage(PlayerAvatar victim, float successQ)
    {
        player.avatar.CmdSendDamage(victim.netId, info.Damage * successQ, successQ);
        //victim.CmdGetDamage(info.Damage * successQ, player.Nik, successQ); (WeaponHandler has no netView)

        Debug.Log(player.Nik + " Hitted " + victim.player.Nik);

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

    IEnumerator WaitAnd(Action whatNext, float awaitTime)
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