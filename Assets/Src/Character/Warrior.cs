using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : Player
{
    void Awake()
    {
        moveSpeed = 15.0f;
        maxStamina.Value = 130;
        curStamina.Value = 130;
        catchSkill = new Catch();
        skills.Add(new Boost());
        skills.Add(new Knockback());
    }
}
