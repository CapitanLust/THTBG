using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour {

    public GameObject preloadData, netManager;

    public void Start()
    {
        DontDestroyOnLoad(preloadData);
        DontDestroyOnLoad(netManager);

        SceneManager.LoadScene(1);
    }

}
