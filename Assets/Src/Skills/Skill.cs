using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// generic Skill class
public abstract class Skill
{
    public float cooldown;

    public abstract void useSkill(Player player);
    
}

// Teleport skill
public class Teleport : Skill
{
    public Teleport()
    {
        cooldown = 6.0f;
    }

    public override void useSkill(Player player)
    {
        Debug.Log("Teleport skill is used");
        Debug.Log(player.transform.position.ToString());
        player.transform.position += player.transform.forward * 2.0f;
        Debug.Log(player.transform.position.ToString());
    }
}

