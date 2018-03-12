using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Equipment // : ScriptableObject // not anymore
{
    public string Name;
    public float Area;
    public float Distance;

    public float Radius;

    /// <summary>
    /// variable responsible for: is
    /// equipment influence effective depends
    /// on a distance to subject
    /// </summary>
    public bool Diffusing = false;
    public bool AreaSizeDependsOnDistance = false;
    // TODO to have own cursor
}

//[CreateAssetMenu(menuName = "Equipment/Weapon")]
[Serializable]
public class Weapon : Equipment
{
    public int Damage;
    public int Ammo;
    public float ReloadTime; // TODO it just for info. Time will be counted in animations
}

//[CreateAssetMenu(menuName = "Equipment/Weapon")]
[Serializable]
public class Gadget : Equipment
{
    public float Effect;
    public int Turns;
}

