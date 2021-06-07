using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bowman : Player
{
    private void Awake()
    {
        moveSpeed = 15;
        catchSkill = new Catch();
        skills.Add(new Smoke());
        skills.Add(new Reach());
    }
}
