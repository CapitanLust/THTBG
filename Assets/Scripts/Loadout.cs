using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class Loadout    : IInitialState
{

    [SyncVar]
    public string WeaponName;
    [SyncVar]
    public string Gadget1Name;
    [SyncVar]
    public string Gadget2Name;

    public WeaponHandler WeaponHandler { get; set; }
    // as property -- to not sync by .networking

    //public GadgetHandler


    public void Inflate(PlayerAvatar avatar, Data data) // TODO variable?
    {
        var weaponPrefab = Resources.Load<WeaponHandler>
            ("Prefabs/Equipment/Weapons/" + WeaponName);

        WeaponHandler = MonoBehaviour.Instantiate<WeaponHandler>(weaponPrefab);

        WeaponHandler.transform.SetParent(avatar.transform, false);

        WeaponHandler.Init( data.GetWeaponByName(WeaponName), avatar.player );
    }


    #region IInitialState implementation

    int fixedMag;

    public void FixateState() 
    {
        fixedMag = WeaponHandler.CurAmmo;
    }
    public void ReturnToInitialTurnState()
    {
        WeaponHandler.CurAmmo = fixedMag;
    }

    #endregion

}
