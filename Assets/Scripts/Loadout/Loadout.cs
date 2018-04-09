using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//TODO rename class

[Serializable]
public class Loadout : IInitialState
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
        var weapon = data.GetWeaponByName(WeaponName);

        WeaponHandler = MonoBehaviour.Instantiate<WeaponHandler>(weaponPrefab, avatar.weaponTransformContainer);

        //WeaponHandler.transform.SetParent(weaponTransformContainer, false);

        WeaponHandler.Init(weapon, avatar.player );

        InflateAnim(avatar);
    }

    void InflateAnim(PlayerAvatar avatar)
    {
        var overrideController = Resources.Load<AnimatorOverrideController>
            ("OverrideControllers/" + WeaponName);

        avatar.anim.runtimeAnimatorController = overrideController;
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
