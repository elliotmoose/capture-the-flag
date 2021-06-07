using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CatchSphere : NetworkBehaviour
{
    private Player player;

    private void Start()
    {
        player = transform.parent.GetComponent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Player target = other.gameObject.GetComponent<Player>();

        if (target != null)
        {
            if (player.team != target.team)
            {
                if (target.IsCatchable())
                {
                    GameManager.Instance.Imprison(target, player);
                }
                // else if (player.IsCatchable()){ //i can imprison myself
                //     player.GetJail().Imprison(player);
                // }
            }
            else if (player.team == target.team)
            {
                GameManager.Instance.Release(target, player);
            }
        }
    }
}
