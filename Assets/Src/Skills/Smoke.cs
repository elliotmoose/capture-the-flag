using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : Skill
{
    private float duration = 10.0f;

    public Smoke()
    {
        cooldown = 10.0f;
        name = "Smoke";
    }

    public override void UseSkill(Player player)
    {
        SmokeEffect effect = new SmokeEffect(player, duration);
        player.TakeEffect(effect);
    }
    
}

public class SmokeEffect : Effect
{
    private GameObject smoke;


    public SmokeEffect(Player _target, float duration) : base(_target)
    {
        this.duration = duration;
        this.name = "SMOKE_EFFECT";
        GameObject smokeObj = GameObject.Find("GameManager").GetComponent<PrefabsManager>().smoke;
        this.smoke = GameObject.Instantiate(smokeObj, _target.transform.position, Quaternion.identity);
    }

    public override void OnEffectApplied()
    {
        this.smoke.GetComponent<NetworkObject>().Spawn();
    }

    public override void OnEffectEnd()
    {
        PrefabsManager.Destroy(this.smoke);
    }

}