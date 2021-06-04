using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// generic Skill class
public abstract class Skill
{
    public float cooldown;
    public string name;

    public abstract void UseSkill(Player player);
    
}