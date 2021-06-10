using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

// Teleport skill
public class Teleport : Skill
{
    private float teleportFactor = 25.0f;

    public Teleport()
    {
        cooldown = 6.0f;
        name = "Teleport";
    }

    public override void UseSkill(Player player)
    {
        Debug.Log(name + " skill is used");

        TeleportEffect effect = new TeleportEffect(player, teleportFactor);
        player.TakeEffect(effect);

    }
}

public class TeleportEffect : Effect
{
    private GameObject teleport;
    private float teleportFactor;
    private Collider col;
    private Renderer[] rends;

    private float animation_duration = 0.4f;
    private bool moved = false;

    public TeleportEffect(Player _target, float teleportFactor) : base(_target)
    {
        this.duration = 1.2f;
        this.teleportFactor = teleportFactor;
        this.name = "TELEPORT_EFFECT";
        this.teleport = GameObject.Find("GameManager").GetComponent<PrefabsManager>().teleportField;
        this.col = _target.GetComponent<Collider>();
        this.rends = _target.GetComponentsInChildren<Renderer>();

    }
    public override void OnEffectApplied()
    {
        GameObject.Instantiate(teleport, _target.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        
        col.enabled = false;
        
    }

    public override void UpdateEffect()
    {
        if (age <= animation_duration)
        {
            float progress = age / animation_duration;

            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                rend.material.SetVector("_direction", new Vector3(0f, 1.0f, 0f));
                rend.material.SetFloat("_dissolved", progress*2-0.5f); // dissolved is from -0.5 to 1.5
            }
        }
        else if (age >= duration - animation_duration)
        {
            if (!moved)
            {
                _target.transform.position += _target.transform.forward * teleportFactor;
                GameObject.Instantiate(teleport, _target.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
                moved = true;
            }

            float progress = (age - (duration - animation_duration)) / animation_duration;
            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                rend.material.SetVector("_direction", new Vector3(0f, -1.0f, 0f));
                rend.material.SetFloat("_dissolved", 1 - (progress * 2 - 0.5f));
            }
        }
    }

    public override void OnEffectEnd()
    {
        
        col.enabled = true;
        
    }
}