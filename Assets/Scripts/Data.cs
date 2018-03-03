using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour {

    // TODO or better to store data in static class?
    //  or in xml and deserialize?
    //  or in database initially and parse? Not. DB is bad for debugging after release

    public Weapon[] Weapons;

    public Weapon GetWeaponByName(string name)
    {
        foreach (var w in Weapons)
            if (w.Name == name) return w;
        return null;
    }

    // !!! /\ There's some troubles with Scriptableobjects. Thet always lost, etc
    // temporary, will implement data by static class


        



}
