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

    protected LocalPlayer _target;

    public Effect(LocalPlayer target)
    {
        this._target = target;
    }

    protected virtual bool ShouldEffectEnd() {
        return age >= duration;
    }
    public void Update()
    {
        UpdateCooldown();
        if (!ShouldEffectEnd())
        {
            effectEnded = false;
            UpdateEffect();
            //Debug.Log(_target.GetMoveSpeed().ToString());
        }
        else
        {
            OnEffectEnd();
            effectEnded = true;
        }
    }

    public void FixedUpdate() {
        if(!ShouldEffectEnd()) {
            FixedUpdateEffect();
        }
    }

    public LocalPlayer GetTarget()
    {
        return _target;
    }

    void UpdateCooldown()
    {
        age += Time.deltaTime;
    }

    public virtual void OnEffectApplied() { }
    public virtual void UpdateEffect() { }
    public virtual void FixedUpdateEffect() { }
    public virtual void OnEffectEnd() { }

}
