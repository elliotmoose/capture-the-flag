using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Reach : Skill
{   

    public Reach()
    {
        cooldown = 12.0f;
        name = "Laser Beam";
        this.icon = PrefabsManager.Instance.extendedReachIcon;
        this.description = "Fires a beam forward, exercising its Catch in a straight line ahead";

    }
    public override void UseSkill(LocalPlayer player)
    {
        ReachEffect reachEffect = new ReachEffect(player);
        player.TakeEffect(reachEffect);
    }
}

public class ReachEffect : Effect
{
    private string animation = "Reach";
    private bool finished = false;
    public float referenceAngle; 

    public ReachEffect(LocalPlayer _target) : base(_target)
    {
        this.name = "REACH_EFFECT";
        this.duration = 0.8f;
        this.referenceAngle = _target.transform.rotation.eulerAngles.y;
        _target.OnAnimationRelease += OnAnimationRelease;
    }

    public override void OnEffectApplied()
    {
        _target.GetComponent<Animator>().SetBool("IsReach", true);
    }
    public void OnAnimationRelease(string animationName)
    {
        if (animationName != animation) return;
        _target.GetComponent<Animator>().SetBool("IsReach", false);

    }

    public override void OnEffectEnd()
    {
        
    }

}
