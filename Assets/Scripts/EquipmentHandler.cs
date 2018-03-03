using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentHandler : MonoBehaviour {}

public class WeaponHandler : EquipmentHandler
{
    public Weapon info;

    public int CurMag;

    public void Init(Weapon info)
    {
        this.info = info;
        CurMag = info.Mag;
    }

    public virtual void Shoot()
    {
        CurMag--;
    }

}