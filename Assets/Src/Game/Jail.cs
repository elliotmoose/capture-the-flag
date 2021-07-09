using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Jail : MonoBehaviour
{    
    public float jailSize = 10; //jail radius
    public Color color;

    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem.ShapeModule shape = GetComponentInChildren<ParticleSystem>().shape;
        shape.radius = jailSize;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = GetComponentInChildren<ParticleSystem>().colorOverLifetime;
        colorOverLifetime.color = color;
    }
}
