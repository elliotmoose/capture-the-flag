using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserker : LocalPlayer
{
    void Awake()
    {
        this.SetMaxStamina(130);            
        moveSpeed = 18.0f;
        catchSkill = new Catch();
        skills.Add(new Boost());
        skills.Add(new Knockback());
    }
}
