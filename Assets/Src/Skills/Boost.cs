using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : Skill
{
    private float boost = 2.0f;

    public Boost()
    {
        cooldown = 6.0f;
        name = "Boost";
    }

    public override void UseSkill(Player player)
    {
        Debug.Log(name + " skill is used");
        Rigidbody playerBody = player.gameObject.GetComponent<Rigidbody>();
        playerBody.AddForce(player.transform.forward * boost * player.GetMoveSpeed(), ForceMode.Impulse);
        
    }

}
