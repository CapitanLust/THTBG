using System;
using System.Collections;
using UnityEngine;

public class WeaponHandler : EquipmentHandler
{
    public Weapon info;

    public int curAmmo;
    public int CurAmmo
    {
        get { return curAmmo; }
        set
        {
            curAmmo = value;

            if(player.hasAuthority) // TODO check performance
                player.ui.UpdateWeaponPanel_Ammo(this);
        }
    }

    public Animator anim,
                    animAvatar;
    public Transform bulletPrefab,
                     bulletSpawn;

    public Player player;

    public Action onShoted, onReloaded;

    TurnAction tAction;

    public void Init(Weapon info, Player player)
    {
        this.player = player;

        this.info = info;
        CurAmmo = info.Ammo;

        animAvatar = player.avatar.anim;
    }

    public void Shoot(TurnAction tAction, Action callback)
    {
        onShoted = callback;
        Shoot(tAction);
    }

    // TODO change arguments to link to Turn
    public virtual void Shoot(TurnAction tAction /*Vector3 point, float successQ*/)
    {
        this.tAction = tAction;

        tAction.ActionStarted = true;

        if (CurAmmo > 0)
        {
            VisualShot(tAction.Point);
            CurAmmo--;

            // Next -- Anim_OnShoted
        }
        else
            StartCoroutine(WaitAnd(() => {
                tAction.ActionEnded = true;
                if (onShoted != null)
                    onShoted();
            }, .4f));
        // TODO else *click*

    }

    protected virtual void ProcessShoot()
    {
        if (player.hasAuthority)
            MeasureAftermath();

        tAction.ActionEnded = true;
    }

    public virtual void MeasureAftermath()
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


    public void Reload(TurnAction tAction, Action callback)
    {
        onReloaded = callback;
        Reload(tAction);
    }

    public virtual void Reload(TurnAction tAction)
    {
        this.tAction = tAction;

        tAction.ActionStarted = true;
        anim.SetTrigger("Reload");
        animAvatar.SetTrigger("Reload");

        // Next -- Anim_OnReloaded
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
        animAvatar.SetTrigger("Shoot");

        var newBull = Instantiate(bulletPrefab, bulletSpawn);
        newBull.SetParent(null);
        //newBull.Rotate() -- random spread or aimed to the victim avatar
    }

    public void Anim_OnShoted()
    {
        if (tAction.turn.Performing)
            ProcessShoot();
        else
            tAction.ActionEnded = true;


        if (onShoted != null)
            onShoted();
    }

    public void Anim_OnReloaded()
    {
        if (CurAmmo != 0)
            CurAmmo = info.Ammo + 1;
        else
            CurAmmo = info.Ammo;

        tAction.ActionEnded = true;


        if (onReloaded != null)
            onReloaded();
    }

}