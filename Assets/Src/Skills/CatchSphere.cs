using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CatchSphere : MonoBehaviour
{
    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //we only check for contact on the server
        if(!NetworkManager.Singleton.IsServer) return;

        Player caster = transform.GetComponentInParent<Player>();
        Player target = other.gameObject.GetComponent<Player>();

        if (target != null)
        {
            target.ServerContact(caster);
        }
    }
}
