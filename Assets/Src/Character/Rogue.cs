using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : Player
{    
    private void Awake()
    {
        if(IsServer) {
            moveSpeed = 15;
        }
        
        catchSkill = new CloneCatch();
        skills.Add(new Invisibility());
    }
}
