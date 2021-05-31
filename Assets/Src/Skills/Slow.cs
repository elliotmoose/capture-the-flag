using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AOE Slow skill
public class Slow : Skill
{
    private float radius = 5.0f; // radius of effect
    private float percentageDecrease = 1.3f;
    private float duration = 3.0f;

    public Slow()
    {
        cooldown = 6.0f;
        name = "AOE Slow";

    }

    public override void UseSkill(Player player)
    {
        Debug.Log(name + " skill is used");
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);
        
        foreach (Collider c in hitColliders)
        {
            
            Player target = c.gameObject.GetComponent<Player>();
                
            if (target != null && player != target)
            {
                SlowEffect effect = new SlowEffect(target, percentageDecrease, duration);
                target.TakeEffect(effect);
            }

            
        }
    }
}


public class SlowEffect : Effect
{
    public float percentageDecrease = 0;

    public SlowEffect(Player _target, float percentageDecrease, float duration) :base(_target)
    {
        this.percentageDecrease = percentageDecrease; // float > 1.0f
        this.duration = duration;
        this.name = "SLOW_EFFECT";
    }

    public override void OnEffectApplied()
    {
        float newSpeed = _target.GetMoveSpeed() / percentageDecrease;
        _target.SetMoveSpeed(newSpeed);
        
    }

    public override void OnEffectEnd()
    {
        // revert movement speed
        float originalSpeed = _target.GetMoveSpeed() * percentageDecrease;
        _target.SetMoveSpeed(originalSpeed);
    }

}