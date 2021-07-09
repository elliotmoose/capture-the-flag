using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : Skill
{
    private float duration = 10.0f;

    public Smoke()
    {
        cooldown = 10.0f;
        name = "Smoke";
    }

    public override void UseSkill(LocalPlayer player)
    {
        SmokeEffect effect = new SmokeEffect(player, duration);
        player.TakeEffect(effect);
    }
    
}

public class SmokeEffect : Effect
{
    private string animation = "Smoke";
    private bool finished = false;

    public SmokeEffect(LocalPlayer _target, float duration) : base(_target)
    {
        this.duration = duration;
        this.name = "SMOKE_EFFECT";
        _target.OnAnimationStart += OnAnimationStart;
        _target.OnAnimationEnd += OnAnimationEnd;
    }

    public override void OnEffectApplied()
    {
        _target.GetComponent<Animator>().SetBool("IsSmoke", true);
    }
    public void OnAnimationStart(string animationName)
    {
        if (animationName != animation) return;
    }

    public void OnAnimationEnd(string animationName)
    {
        if (animationName != animation) return;

        _target.GetComponent<Animator>().SetBool("IsSmoke", false);
        finished = true;
    }
    protected override bool ShouldEffectEnd()
    {
        return finished;
    }

    public override void OnEffectEnd()
    {

    }

}