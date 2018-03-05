using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bull : MonoBehaviour {

    private void Update()
    {
        transform.position += transform.forward * 1.4f;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        // TODO also draw decal
    }

}
