using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CatchSphere : MonoBehaviour
{
    private LocalPlayer player;

    private void Start()
    {
        player = transform.GetComponentInParent<LocalPlayer>();        
    }

    private void OnTriggerEnter(Collider other)
    {
        LocalPlayer target = other.gameObject.GetComponent<LocalPlayer>();

        if (target != null)
        {
            target.Contact(player);
        }
    }
}
