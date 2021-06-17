using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserker : Player
{
    void Awake()
    {
        if(IsServer) {
            moveSpeed = 15.0f;
            this.SetMaxStamina(130);            
        }
        catchSkill = new Catch();
        skills.Add(new Boost());
        skills.Add(new Knockback());
    }
}
