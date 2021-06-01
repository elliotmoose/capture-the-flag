using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : Skill
{
    private float duration = 3.0f;

    public Invisibility()
    {
        cooldown = 8.0f;
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

    public InvisEffect(Player _target, float duration) : base(_target)
    {
        this.duration = duration;
        this.name = "INVIS_EFFECT";
                
    }

    public override void OnEffectApplied()
    {
        _target.SetInvis(true);
    }

    public override void OnEffectEnd()
    {
        _target.SetInvis(false);
    }

}