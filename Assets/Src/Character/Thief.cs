using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : Player
{    
    private void Awake()
    {
        moveSpeed = 15;
        catchSkill = new Catch(catchRadius);
        skills.Add(new Invisibility());
        skills.Add(new Smoke());

    }
}
