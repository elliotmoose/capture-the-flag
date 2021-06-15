using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : Skill
{
    private float distance = 30.0f;
    private float timeTaken = 0.4f;

    public Boost()
    {
        cooldown = 6.0f;
        name = "Boost";
    }

    public override void UseSkill(Player player)
    {
        Debug.Log(name + " skill is used");
        PushEffect effect = new PushEffect(player, player.transform.forward, distance, timeTaken);
        player.TakeEffect(effect);

    }

}
