using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Catch : Skill
{   
    LocalPlayer caster;
    public Catch()
    {
        cooldown = 1.0f;
        name = "Catch";        
    }

    public override void UseSkill(LocalPlayer caster)
    {
        this.caster = caster;
        caster.GetComponent<Animator>().SetBool("IsCatching", true);
        caster.OnAnimationStart += OnAnimationStart;      
        caster.OnAnimationEnd += OnAnimationEnd;      
    }

    public void OnAnimationStart(string animationName) {
        if(animationName != name) return;
        caster.transform.Find("Catch").localScale = Vector3.one * caster.syncPlayer.GetCatchRadius();
    }
    
    public void OnAnimationEnd(string animationName) {
        if(animationName != name) return;
        caster.GetComponent<Animator>().SetBool("IsCatching", false);
        caster.OnAnimationStart -= OnAnimationStart;      
        caster.OnAnimationEnd -= OnAnimationEnd;      
    }
}