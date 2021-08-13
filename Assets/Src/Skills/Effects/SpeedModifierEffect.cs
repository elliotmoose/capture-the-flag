using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Skill effect on player that lasts for a period of time
public class SpeedModifierEffect : Effect {
    public float addSpeed = 0;
    public float scaleSpeed = 1;
    public SpeedModifierEffect(LocalPlayer target, float addSpeed=0, float scaleSpeed=1) : base(target) {
        this.addSpeed=addSpeed;
        this.scaleSpeed=scaleSpeed;
    }
}