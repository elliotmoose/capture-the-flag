using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZoneEffect : SpeedModifierEffect {

    public SpeedZoneEffect(LocalPlayer target, float addSpeed) : base(target, addSpeed: addSpeed) {
        this.name = "SPEED_ZONE_EFFECT";
    }

    protected override bool ShouldEffectEnd()
    {
        //effects end when exit
        return false;
    }
}