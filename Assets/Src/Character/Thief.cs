using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : Player
{
    private void Awake()
    {
        moveSpeed = 15;
        skills.Add(new Invisibility());
    }
}
