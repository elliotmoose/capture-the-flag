using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lancer : LocalPlayer
{
    private void Awake()
    {
        if(IsServer) {
            this.syncPlayer.SetMaxStamina(80);
        }
        
        moveSpeed = 18;
        catchSkill = new Catch();
        skills.Add(new Smoke());
        skills.Add(new Reach());
    }
}
