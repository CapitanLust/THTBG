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


    public bool performing = false;

    public Action update;

    public PlayerAvatar avatar;


    public GameManager gameManager;
    public GameManager.UI ui;

    #region Initiative part

    private void Awake()
    {
        update = Update_Empty;
    }

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
    }


    #region input

    void Update()
    {
        update();
    }

    public void Update_Empty() { }
    public void Update_Lobby()
    {

    }
    public void Update_Game()
    {
        // todo check 'if' performance
        if (Input.GetKeyDown(KeyCode.D))
            foreach (var p in gameManager.players)
                if (p.batch != null) Debug.Log(p.Nik 
                    + " -- " + p.batch.PlayerOwnerNik);

        if (!isReady) 
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log(batch.Serialized.Length);
                CmdSetReady(batch.Serialized);
            }
            else
            {
                if (!isAlive)
                {
                    ListenMouseForSpawn();
                }
                else
                    avatar.update();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log(batch.Serialized.Length);
                //batch = null;
                //avatar.Cancel;// TODO
            }
        }
    }

    void ListenMouseForSpawn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // TODO check for availabilty of spawn. Like near physical borders and other player spawns
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.tag == "Floor")
                SetSpawn(hit.point);
        }
    }
    void SetSpawn(Vector3 spawnPos)
    {
        batch = new SpawnInfo() { Point = spawnPos, PlayerOwnerNik = Nik };

        // yes, gettin mousePos from here is declining some universallity, but idc
        ui.MarkSpawnPoint(Input.mousePosition);
    }

    #endregion


    public void OnStartMatch ()
    {

    }


    #endregion


    #region UI
    

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

    public string PlayerOwnerNik;

    // it was designed for abstract. But we can't use abst with u.networking
    public virtual void Perform() {}


    [SerializeField]
    float x = 0, y = 0, z = 0;
    public Vector3 Point
    {
        get { return new Vector3(x, y, z); }
        set { x = value.x; y = value.y; z = value.z; }
    }

}

[Serializable]
public class SpawnInfo : Batch
{
    public override void Perform()
    {
        Debug.Log("SpawnInfo: " + Point);

        //GameManager.Spawn(Point, PlayerOwnerNik);
        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager>(); // TODO performance?
        gameManager.Spawn(Point, PlayerOwnerNik);
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