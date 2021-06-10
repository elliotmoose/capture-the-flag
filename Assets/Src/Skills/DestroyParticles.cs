using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DestroyParticles : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, GetComponent<ParticleSystem>().main.duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
