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
        cooldown = 6.0f;
        name = "Knockback";
        this.icon = PrefabsManager.Instance.knockbackIcon;
        this.description = "Berserker explodes in rage, pushing surrounding N.O.Ds back and disabling for a short duration";
    }

    public override void UseSkill(LocalPlayer player)
    {
        this.player = player;
        player.OnAnimationStart += OnAnimationStart;
        player.OnAnimationEnd += OnAnimationEnd;
        player.OnAnimationCommit += OnAnimationCommit;
        player.GetComponent<Animator>().SetBool("IsKnockback", true);
        player.SetDisabled(true);

        Debug.Log(name + " skill is used");
        

    }
    public void OnAnimationStart(string animationName)
    {
        if (animationName != animation) return;
    }

    public void OnAnimationEnd(string animationName)
    {
        if (animationName != animation) return;

        player.GetComponent<Animator>().SetBool("IsKnockback", false);
        player.SetDisabled(false);

    }

    public void OnAnimationCommit(string animationName)
    {
        if (animationName != animation) return;

        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);
        Debug.Log(hitColliders.Length);
        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();

            if (target != null && player != target)
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
