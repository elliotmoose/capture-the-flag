using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Adept : Player
{
    void Awake()
    {
        if(IsServer) {
            moveSpeed = 15;            
            this.SetCatchRadius(10);
        }
        catchSkill = new Catch();
        skills.Add(new Teleport());
        skills.Add(new Slow());
    }    
}
