using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Skill effect on player that lasts for a period of time
public abstract class Effect
{
    public float duration; // duration the effect is applied for
    public float age = 0.0f; // how long the effect has been applied
    public bool effectEnded = false;
    public string name;

    protected Player _target;

    public Effect(Player target)
    {
        this._target = target;
    }

    public void Update()
    {
        UpdateCooldown();
        if (age < duration)
        {
            effectEnded = false;
            UpdateEffect();
            //Debug.Log(_target.GetMoveSpeed().ToString());
        }
        else
        {
            effectEnded = true;
            OnEffectEnd();
        }
    }

    public Player GetTarget()
    {
        return _target;
    }

    void UpdateCooldown()
    {
        age += Time.deltaTime;
    }

    public virtual void OnEffectApplied() { }
    public virtual void UpdateEffect() { }
    public virtual void OnEffectEnd() { }

}
