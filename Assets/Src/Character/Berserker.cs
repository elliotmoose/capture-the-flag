using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserker : LocalPlayer
{
    void Awake()
    {
        if(IsServer) {
            this.syncPlayer.SetMaxStamina(130);            
        }
        
        moveSpeed = 15.0f;
        catchSkill = new Catch();
        skills.Add(new Boost());
        skills.Add(new Knockback());
    }
}
