using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mage : Player
{
    void Awake()
    {
        moveSpeed = 15;
        catchSkill = new Catch(catchRadius);
        skills.Add(new Teleport());
        skills.Add(new Slow());
    }
    
}
