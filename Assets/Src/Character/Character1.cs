using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Character1 : Player
{
    void Awake()
    {
        moveSpeed = 15;
        skills.Add(new Teleport());
    }
    
}
