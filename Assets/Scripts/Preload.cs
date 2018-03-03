using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour {

    public GameObject preloadData;

    public void Start()
    {
        DontDestroyOnLoad(preloadData);

        SceneManager.LoadScene(1);
    }

}
