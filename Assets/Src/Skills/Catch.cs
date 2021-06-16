using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Catch : Skill
{
    protected float catchRadius = 8;
    public Catch(float catchRadius=8)
    {
        this.catchRadius = catchRadius;
        cooldown = 1.0f;
        name = "Catch";
        
    }

    public override void UseSkill(Player player)
    {
        CatchEffect catchEffect = new CatchEffect(player, catchRadius);
        player.TakeEffect(catchEffect);
    }
}

public class CatchEffect : Effect
{
    private float radius;
    private GameObject catchField;
    private Renderer rend;
    private Color color;
    
    public CatchEffect(Player _player, float radius) : base(_player)
    {
        this.radius = radius;
        this.duration = 0.2f;
        this.name = "CATCH_EFFECT";
        this.catchField = GameObject.Instantiate(PrefabsManager.Instance.catchField, _player.transform.Find("model").transform.position, Quaternion.identity);
        this.catchField.transform.parent = _player.transform;
        rend = this.catchField.GetComponent<Renderer>();
        if (_player.GetTeam() == Team.RED)
        {
            color = new Color(243, 31, 0);
            rend.material.SetColor("_emission", color);
        }
        else
        {
            color = new Color(0, 225, 243);
            rend.material.SetColor("_emission", color);
        }
        
    }

    public override void OnEffectApplied()
    {
        this.catchField.GetComponent<NetworkObject>().Spawn();
    }

    public override void OnEffectEnd()
    {
        PrefabsManager.Destroy(this.catchField);
    }

    public override void UpdateEffect()
    {
        float current_radius = this.radius / this.duration * this.age;
        this.catchField.transform.localScale = new Vector3(current_radius, current_radius, current_radius);
        float alpha = 1.0f - (this.age / this.duration);
        rend.material.SetFloat("_alpha", alpha);
    }
}