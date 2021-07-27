using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lancer : LocalPlayer
{
    private void Awake()
    {
        this.SetMaxStamina(80);
        moveSpeed = 18;
        catchSkill = new Catch();
        skills.Add(new StaminaBuff());
        skills.Add(new Reach());
    }
}
