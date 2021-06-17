using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Catch : Skill
{
    public Catch()
    {
        cooldown = 1.0f;
        name = "Catch";        
    }

    public override void UseSkill(Player player)
    {
        CatchEffect catchEffect = new CatchEffect(player, player.GetCatchRadius());
        player.TakeEffect(catchEffect);
    }
}

public class CatchEffect : Effect
{
    private float radius;
    private GameObject catchField;
    private Renderer rend;
    private Color color;
    
    private string animation = "Catch";
    private bool finished = false;
    public CatchEffect(Player _player, float radius) : base(_player)
    {        
        this.duration = 0.2f;
        this.name = "CATCH_EFFECT";

        _player.OnAnimationStart += OnAnimationStart;      
        _player.OnAnimationEnd += OnAnimationEnd;      
    }

    public void OnAnimationStart(string animationName) {
        if(animationName != animation) return;
    }
    
    public void OnAnimationEnd(string animationName) {
        if(animationName != animation) return;

        _target.GetComponent<Animator>().SetBool("IsCatching", false);
        finished = true;
    }

    protected override bool ShouldEffectEnd()
    {
        return finished;
    }

    public override void OnEffectApplied()
    {
        _target.GetComponent<Animator>().SetBool("IsCatching", true);
    }

    public override void OnEffectEnd()
    {
        
    }

    public override void UpdateEffect()
    {
        
    }
}