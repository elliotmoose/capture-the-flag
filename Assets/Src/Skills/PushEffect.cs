using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushEffect : Effect
{
    private Vector3 initialPos;
    private Vector3 finalPos;
    private Vector3 direction;

    public PushEffect(Player _target, Vector3 direction, float distance, float duration) : base(_target)
    {
        this.duration = duration;
        this.direction = direction;
        this.name = "PUSH_EFFECT";
        initialPos = _target.transform.position;
        finalPos = initialPos + direction.normalized * distance;
        finalPos = new Vector3(finalPos.x, initialPos.y, finalPos.z); // make sure height is same

    }

    //returns a quadratic function that eases the input from 0 -> 1 
    float EaseOutQuadratic(float x)
    {
        float y = 1 - Mathf.Pow(x - 1, 2);
        return y;
    }

    //returns a circle function that eases the input from 0 -> 1 
    //\sqrt{\left(1-\left(x-1\right)^{2}\right)}
    float EaseOutCircular(float x)
    {
        float y = Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
        return y;
    }

    public override void FixedUpdateEffect()
    {
        // float progress = age / duration;
        // Vector3 targetPosition = Vector3.Lerp(initialPos, finalPos, EaseOutQuadratic(progress));
        // Vector3 delta = (targetPosition - _target.transform.position);
        float speed = 80f;
        _target.GetComponent<CharacterController>().Move(this.direction.normalized * Time.fixedDeltaTime * speed);
    }

    public override void OnEffectApplied()
    {
        _target.SetDisabled(true);

    }

    public override void OnEffectEnd()
    {
        _target.SetDisabled(false);
    }

}