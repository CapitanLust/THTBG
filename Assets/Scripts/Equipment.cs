using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Equipment // : ScriptableObject // not anymore
{

    public float Radius;

    public string Name;

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
    public float Damage;
    public int Mag;
    public float ReloadTime; // TODO it just for info. Time will be counted in animations
}

//[CreateAssetMenu(menuName = "Equipment/Weapon")]
[Serializable]
public class Gadget : Equipment
{
}

