using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

public class Player : NetworkBehaviour {

    [SyncVar]
    public string Nik;

    [SyncVar]
    public Cmn.EPlayerColor Color;

    [SyncVar(hook = "Registrate")]
    public bool ReadyToRegistrate = false;

    [SyncVar] public bool isAlive = false;
    [SyncVar(hook = "OnIsReadyChanged")]
    public bool isReady = false;

    //[SyncVar (hook="SerializeBatch")]
    // sync changed to cmd-rpc logic
    //public byte[] serializedBatch;
    [SyncVar]
    public Batch batch;

    public GameManager gameManager;
    public GameManager.UI ui;

    #region Initiative part

    void Registrate(bool ready)
    {
        if (ReadyToRegistrate = ready)
            GameObject.Find("Lobby").GetComponent<Lobby>().AddPlayer(this);
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // though object creates only on this scene. Anyway:
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            CmdSetMyParameters(Cmn.Nik, Cmn.PlayerColor);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("a1");

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            var lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
            if (ReadyToRegistrate && !lobby.players.Contains(this))
                lobby.AddPlayer(this);
        }

        DontDestroyOnLoad(gameObject);
    }



    [Command]
    void CmdSetMyParameters(string nik, Cmn.EPlayerColor color)
    {
        Nik = nik;
        Color = color;
        ReadyToRegistrate = true;
    }

    #endregion


    #region Playing

    // # START OF LOGIC
    [Command] // server side
    public void CmdSetReady(byte[] serializedBatch)
    {
        isReady = true;
        // next -- execution goes to isReady sync hook (method)

        // sync batch 
        this.batch = Batch.Deserialize(serializedBatch);
        //this.serializedBatch = serializedBatch;
        //RpcSyncBatch(serializedBatch);
        // ahaha. Я тут возился с синхронизацией + сериализацией абстрактных классов
        // а оказалось я изначально все делал правильно, но
        // в методе десериализации стояло: return null
        // -_- ...  :D

        // still only server
        gameManager.OnPlayerReady();
    }
    public void OnIsReadyChanged(bool ready) // client side
    {
        isReady = ready;
        ui.UpdPlayerList(gameManager.players);

        Debug.Log("check for repeating action");
    }
    
    /*
    public void SerializeBatch(byte[] barray)
    {
    }
    [ClientRpc]
    public void RpcSyncBatch(byte[] barray)
    {
        //serializedBatch = barray;
        Debug.Log("Batch is " + (batch!=null));
        batch = Batch.Deserialize(barray);
        Debug.Log("Batch is " + (batch!=null));
        Debug.Log("rpc sync batch");
    }*/

    public void Perform()
    {
        Debug.Log("Performing: " + Nik);

        if (batch != null)
            batch.Perform();
        else Debug.Log("n");
    }


    // input part

    private void Update()
    {
        if (!hasAuthority) return; // TODO another way?

        // Temp
        if (!isReady) // temp in if. TODO delegating for like state machine style
        {
            if (Input.GetKeyDown(KeyCode.R))
                if (batch != null)
                    CmdSetReady(batch.Serialized);
                else
                    CmdSetReady(null); // TODO rewrite
            else if (Input.GetKeyDown(KeyCode.S))
                batch = new SpawnInfo() { point = new Vector3(0, 2, 30) };
            else if (Input.GetKeyDown(KeyCode.A))
                batch = new A() { msg = "message yo." };

            if (Input.GetKeyDown(KeyCode.L)) {
                Debug.Log("batch is null: " + (batch == null));
                if(batch!=null) Debug.Log(batch.Serialized.Length);
            }
        }
    }


    #endregion


}


[Serializable]
public class Batch
{
    
    public byte[] Serialized
    {
        get
        {
            return Cmn.SerializeBatch(this);
        }
    }
    public static Batch Deserialize (byte[] barray)
    {
        return Cmn.DeserializeBatch(barray);
    }

    // it was designed for abstract. But we can't use abst with u.networking
    public virtual void Perform() { }

}

[Serializable]
public class SpawnInfo : Batch
{
    [SerializeField]
    public float x=0, y=0, z=0;

    public Vector3 point {
        get { return new Vector3(x, y, z); }
        set { x = value.x; y = value.y; z = value.z; }
    }

    public override void Perform()
    {
        Debug.Log("SpawnInfo: " + point);
    }
}

[Serializable]
public class A : Batch
{
    [SerializeField]
    public string msg;

    public override void Perform()
    {
        Debug.Log("A: " + msg);
    }
}