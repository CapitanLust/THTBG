using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentHandler : MonoBehaviour {}

public class WeaponHandler : EquipmentHandler
{
    public Weapon info;

    public int CurMag;

    void Start()
    {
        CurMag = info.Mag;
    }

    public virtual void Shoot()
    {
        CurMag--;
    }

}