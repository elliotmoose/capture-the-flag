using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaBuff : Skill
{
    private float duration = 2.0f;

    public StaminaBuff()
    {
        cooldown = 9.0f;
        name = "Inductive Charge";
        this.icon = PrefabsManager.Instance.inductiveChargeIcon;
        this.description = "Generates an inductive field, increasing ally N.O.Ds' stamina regeneration while draining enemy N.O.Ds' stamina";
    }

    public override void UseSkill(LocalPlayer player)
    {
        BuffEffect effect = new BuffEffect(player, duration);
        player.TakeEffect(effect);
        
    }
}

public class BuffEffect : Effect
{
    private Animator animator;

    public BuffEffect(LocalPlayer _target, float duration) : base(_target)
    {
        this.duration = duration;
        this.name = "BUFF_EFFECT";
        this.animator = _target.GetComponent<Animator>();

    }

    public override void OnEffectApplied()
    {
        animator.SetBool("IsBuffing", true);
    }

    public override void OnEffectEnd()
    {
        animator.SetBool("IsBuffing", false);
    }

}