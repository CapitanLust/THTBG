using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preload : MonoBehaviour {

    public GameObject[] objectsToNotDestroy;
    public string sceneToLoad;

    public void Start()
    {
        foreach(var go in objectsToNotDestroy)
            DontDestroyOnLoad(go);

        SceneManager.LoadScene(sceneToLoad);
    }
}
