using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : Skill
{
    private float duration = 4.0f;

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
    private Renderer[] rends;
    private float animation_duration = 0.5f;
    private float alphaValue = 0.3f; // how transparent does your team see you
    public InvisEffect(Player _target, float duration) : base(_target)
    {
        this.duration = duration;
        
        this.name = "INVIS_EFFECT";
        this.rends = _target.GetComponentsInChildren<Renderer>();

    }

    public override void OnEffectApplied()
    {
        for (int i = 0; i < this.rends.Length; i++)
        {
            Renderer rend = rends[i];
            if (_target.team == PlayerController.LocalInstance.GetPlayer().team)
            {
                // if same team, appear transparent
                rend.material.SetFloat("_alphaValue", alphaValue);
            }
            else
            {
                // if enemy team, appear invisible
                rend.material.SetFloat("_alphaValue", 0f);

            }
        }

    }

    public override void UpdateEffect()
    {
        if (age <= animation_duration)
        {
            float progress = age / animation_duration;

            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                rend.material.SetFloat("_dissolved", progress);
            }
        }
        else if (age >= duration - animation_duration)
        {
            float progress = (age - (duration - animation_duration)) / animation_duration;
            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                rend.material.SetFloat("_dissolved", 1-progress);
            }
        }
    }


    public override void OnEffectEnd()
    {
        for (int i = 0; i < this.rends.Length; i++)
        {
            Renderer rend = rends[i];
            rend.material.SetFloat("_alphaValue", 1.0f);
        }
    }

}