using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lancer : LocalPlayer
{
    private void Awake()
    {
        this.SetMaxStamina(80);
        baseMoveSpeed = 21;
        catchSkill = new Catch();
        skills.Add(new StaminaBuff());
        skills.Add(new Reach());

    }
}
