using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : Player
{
    void Awake()
    {
        moveSpeed = 15.0f;
        catchSkill = new Catch(catchRadius);
        skills.Add(new Boost());
        skills.Add(new Knockback());
    }
}
