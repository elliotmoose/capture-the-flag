using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DestroyParticles : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
