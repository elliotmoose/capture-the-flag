using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AOE Slow skill
public class Slow : Skill
{
    private float radius = 15.0f; // radius of effect
    private float percentageDecrease = 3.0f;
    private float duration = 2.0f;
    private string animation = "EMP";
    private Animator animator;

    private LocalPlayer caster;
    public Slow()
    {
        cooldown = 8.0f;
        name = "EMP";
        this.icon = PrefabsManager.Instance.empIcon;
        this.description = "The Adept charges up and releases a surge of electromagnetic energy, short circuiting enemy N.O.Ds causing them to be slowed";
    }

    public override void UseSkill(LocalPlayer player)
    {
        caster = player;
        this.animator = player.GetComponent<Animator>();
        caster.OnAnimationStart += OnAnimationStart;
        caster.OnAnimationCommit += OnAnimationCommit;
        caster.OnAnimationRelease += OnAnimationRelease;
        caster.OnAnimationEnd += OnAnimationEnd;
        caster.SetDisabled(true);
        animator.SetBool("IsEMPing", true);
    }

    public void OnAnimationStart(string animationName) {
        if(animationName != animation) return;
        Debug.Log("Animation started: " + animation);
    }

    public void OnAnimationCommit(string animationName) {
        if(animationName != animation) return;
        Trigger();
    }

    public void OnAnimationRelease(string animationName) {
        if(animationName != animation) return;
        caster.SetDisabled(false);
    }
    
    public void OnAnimationEnd(string animationName) {
        if(animationName != animation) return;
        animator.SetBool("IsEMPing", false);
    }
    private void Trigger() {
        Debug.Log(name + " skill is used");
        Collider[] hitColliders = Physics.OverlapSphere(caster.transform.position, radius);
        
        foreach (Collider c in hitColliders)
        {
            LocalPlayer target = c.gameObject.GetComponent<LocalPlayer>();
                
            if (target != null && caster.team != target.team)
            {
                target.TakeNetworkEffect(EffectType.Slow, caster.OwnerClientId);
                // SlowEffect effect = new SlowEffect(target, percentageDecrease, duration);
                // target.TakeEffect(effect);
            }            
        }
    }
}


public class SlowEffect : Effect
{
    public float percentageDecrease = 0;
    private Animator animator;

    public SlowEffect(LocalPlayer _target, float percentageDecrease, float duration) :base(_target)
    {
        this.percentageDecrease = percentageDecrease; // float > 1.0f
        this.duration = duration;
        this.name = "SLOW_EFFECT";
        this.animator = _target.GetComponent<Animator>();
    }

    public override void OnEffectApplied()
    {
        float newSpeed = _target.GetMoveSpeed() / percentageDecrease;
        _target.SetMoveSpeed(newSpeed);
        animator.SetBool("IsSlowed", true);
    }

    public override void OnEffectEnd()
    {
        // revert movement speed
        float originalSpeed = _target.GetMoveSpeed() * percentageDecrease;
        _target.SetMoveSpeed(originalSpeed);
        animator.SetBool("IsSlowed", false);
    }

}