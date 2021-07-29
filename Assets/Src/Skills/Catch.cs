using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Catch : Skill
{   
    LocalPlayer caster;
    string _animationName = "Catch";        
    public Catch()
    {
        cooldown = 1.0f;
        name = "Catch";        
        this.icon = PrefabsManager.Instance.catchIcon;
    }

    public override void UseSkill(LocalPlayer caster)
    {
        this.caster = caster;
        caster.OnAnimationStart += OnAnimationStart;      
        caster.OnAnimationEnd += OnAnimationEnd;      
        caster.GetComponent<Animator>().SetBool("IsCatching", true);

        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, caster.syncPlayer.GetCatchRadius());

        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();
            if(target == caster.syncPlayer) continue; //cannot catch self
            if (target != null)
            {
                target.ClientContact(caster.OwnerClientId);
            }
        }
    }

    public void OnAnimationStart(string animationName) {
        if(animationName != _animationName) return;        
    }

    public void OnAnimationEnd(string animationName) {
        if(animationName != this._animationName) return;
        caster.GetComponent<Animator>().SetBool("IsCatching", false);
        caster.OnAnimationStart -= OnAnimationStart;      
        caster.OnAnimationEnd -= OnAnimationEnd;
    }
}