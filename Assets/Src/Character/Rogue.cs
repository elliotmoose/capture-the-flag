using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : LocalPlayer
{    
    private void Awake()
    {
        moveSpeed = 15;
        catchSkill = new CloneCatch();
        skills.Add(new Invisibility());
    }
}
