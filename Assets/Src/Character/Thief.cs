using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : Player
{    
    private void Awake()
    {
        moveSpeed = 15;
        catchSkill = new CloneCatch(catchRadius);
        skills.Add(new Invisibility());
    }
}
