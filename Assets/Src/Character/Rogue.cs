using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : LocalPlayer
{    
    public static float EVADE_CHANCE = 0.2f;
    private void Awake()
    {
        baseMoveSpeed = 18;
        catchSkill = new CloneCatch();
        skills.Add(new Invisibility());
    }
}
