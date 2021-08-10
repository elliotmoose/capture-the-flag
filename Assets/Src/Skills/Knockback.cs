using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : Skill
{
    private float finalDistance = 15.0f;
    private float timeTaken = 0.3f;
    private float radius = 15.0f;

    LocalPlayer player;
    private string animation = "Knockback";

    public Knockback()
    {
        cooldown = 8.0f;
        name = "Knockback";
        this.icon = PrefabsManager.Instance.knockbackIcon;
        this.description = "Berserker explodes in rage, pushing surrounding N.O.Ds back and disabling them for a short duration";
    }

    public override void UseSkill(LocalPlayer player)
    {
        this.player = player;
        player.OnAnimationStart += OnAnimationStart;
        player.OnAnimationEnd += OnAnimationEnd;
        player.OnAnimationRelease += OnAnimationRelease;
        player.OnAnimationCommit += OnAnimationCommit;
        player.GetComponent<Animator>().SetBool("IsKnockback", true);
        player.SetDisabled(true);

        Debug.Log(name + " skill is used");
        

    }
    public void OnAnimationStart(string animationName)
    {
        if (animationName != animation) return;
    }

    public void OnAnimationRelease(string animationName)
    {
        if (animationName != animation) return;
        player.SetDisabled(false);
    }

    public void OnAnimationEnd(string animationName)
    {
        if (animationName != animation) return;
        player.GetComponent<Animator>().SetBool("IsKnockback", false);
    }

    public void OnAnimationCommit(string animationName)
    {
        if (animationName != animation) return;

        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);
        Debug.Log(hitColliders.Length);
        foreach (Collider c in hitColliders)
        {
            LocalPlayer target = c.gameObject.GetComponent<LocalPlayer>();

            if (target != null && player.team != target.team)
            {
                Debug.Log(target.ToString());
                Vector3 direction = target.transform.position - player.transform.position;
                float currentDistance = direction.magnitude;
                float knockbackDistance = finalDistance - currentDistance;

                target.TakeNetworkEffect(EffectType.Knockback, player.OwnerClientId);
                // PushEffect effect = new PushEffect(target, direction, knockbackDistance, timeTaken);
                // target.TakeEffect(effect);
            }
        }
    }
}

public class StunEffect : Effect
{

    public StunEffect(LocalPlayer _target, float duration) : base(_target)
    {
        this.duration = duration;
        this.name = "STUN_EFFECT";

    }

    public override void OnEffectApplied()
    {
        _target.SetDisabled(true);
        _target.GetComponent<Animator>().SetBool("IsStunned", true);

    }

    public override void OnEffectEnd()
    {
        _target.GetComponent<Animator>().SetBool("IsStunned", false);
        _target.SetDisabled(false);
    }

}

public class KnockbackEffect : PushEffect
{
    private float stunDuration;

    public KnockbackEffect(LocalPlayer _target, Vector3 direction, float distance, float duration, float stunDuration) : base(_target, direction, distance, duration)
    {
        this.stunDuration = stunDuration;

    }

    public override void OnEffectApplied()
    {
        base.OnEffectApplied();
        StunEffect effect = new StunEffect(_target, stunDuration);
        _target.TakeEffect(effect);
    }
}
