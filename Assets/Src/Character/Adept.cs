using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Adept : LocalPlayer
{
    void Awake()
    {
        moveSpeed = 18;            
        if(IsServer) {
            this.syncPlayer.SetCatchRadius(5);
        }
        catchSkill = new Catch();
        skills.Add(new Teleport());
        skills.Add(new Slow());
    }    
}
