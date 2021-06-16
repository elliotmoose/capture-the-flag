using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : Skill
{
    private float duration = 4.0f;

    public Invisibility()
    {
        cooldown = 14.0f;
        name = "Invisibility";
    }

    public override void UseSkill(Player player)
    {
        InvisEffect effect = new InvisEffect(player, duration);
        player.TakeEffect(effect);
    }
}

public class InvisEffect : Effect
{
    private float animation_duration = 0.5f;
    private float alphaValue = 0.3f; // how transparent does your team see you
    public InvisEffect(Player _target, float duration) : base(_target)
    {
        this.duration = duration;
        this.name = "INVIS_EFFECT";
    }

    public override void OnEffectApplied()
    {        
        _target.GetComponent<Animator>().SetBool("IsInvisible", true);
    }

    public override void UpdateEffect()
    {

    }

    public override void OnEffectEnd()
    {   
        _target.GetComponent<Animator>().SetBool("IsInvisible", false);
    }

}