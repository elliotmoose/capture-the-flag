using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lancer : Player
{
    private void Awake()
    {
        if(IsServer) {
            moveSpeed = 18;
            this.SetMaxStamina(80);
        }
        
        catchSkill = new Catch();
        skills.Add(new Smoke());
        skills.Add(new Reach());
    }
}
