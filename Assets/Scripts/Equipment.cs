using System;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : ScriptableObject // important!
{

    public float Radius;

    public string Name;

    // TODO to have own cursor
}

[CreateAssetMenu(menuName = "Equipment/Weapon")]
public class Weapon : Equipment
{
    public float Damage;
    public int Mag;
}

[CreateAssetMenu(menuName = "Equipment/Weapon")]
public class Gadget : Equipment
{
}

