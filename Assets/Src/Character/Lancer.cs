using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lancer : Player
{
    private void Awake()
    {
        moveSpeed = 18;
        maxStamina.Value = 80;
        curStamina.Value = 80;
        catchSkill = new Catch();
        skills.Add(new Smoke());
        skills.Add(new Reach());
    }
}
