using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : Skill
{
    private float distance = 110.0f;
    private float timeTaken = 0.65f;

    public Boost()
    {
        cooldown = 10.0f;
        name = "Boost";
        this.icon = PrefabsManager.Instance.boostIcon;        
        this.description = "Activates thrusters, boosting the Berserker forward with incredible speed";
    }

    public override void UseSkill(LocalPlayer player)
    {
        // Debug.Log(name + " skill is used");
        BoostEffect effect = new BoostEffect(player, player.transform.forward, distance, timeTaken);
        player.TakeEffect(effect);

    }

}

public class BoostEffect : PushEffect
{
    private string animation = "Boost";
    private bool finished = false;

    public BoostEffect(LocalPlayer _target, Vector3 direction, float distance, float duration) : base(_target, direction, distance, duration)
    {
        this.name = "BOOST_EFFECT";
        _target.OnAnimationStart += OnAnimationStart;
        _target.OnAnimationEnd += OnAnimationEnd;
    }

    public void OnAnimationStart(string animationName)
    {
        if (animationName != animation) return;
    }

    public void OnAnimationEnd(string animationName)
    {
        if (animationName != animation) return;

        _target.GetComponent<Animator>().SetBool("IsBoosting", false);
        finished = true;
    }
    protected override bool ShouldEffectEnd()
    {
        return finished;
    }

    public override void OnEffectApplied()
    {
        _target.GetComponent<Animator>().SetBool("IsBoosting", true);
        base.OnEffectApplied();
    }

}