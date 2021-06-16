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
    private Animator animator;
    private string animation = "Teleport";
    private float animation_duration = 0.4f;
    private bool finished = false;

    public TeleportEffect(Player _target, float teleportFactor) : base(_target)
    {
        this.duration = 1.2f;
        this.teleportFactor = teleportFactor;
        this.name = "TELEPORT_EFFECT";
        this.teleport = GameObject.Find("GameManager").GetComponent<PrefabsManager>().teleportField;
        this.col = _target.GetComponent<Collider>();
        this.rends = _target.GetComponentsInChildren<Renderer>();
        this.animator = _target.GetComponent<Animator>();
    }

    public override void OnEffectApplied()
    {
        GameObject.Instantiate(teleport, _target.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        _target.SetDisabled(true);
        col.enabled = false;
        
        animator.SetBool("IsTeleporting", true);
        _target.OnAnimationStart += OnAnimationStart;
        _target.OnAnimationCommit += OnAnimationCommit;
        _target.OnAnimationRelease += OnAnimationRelease;
        _target.OnAnimationEnd += OnAnimationEnd;
    }

    public void OnAnimationStart(string animationName) {
        if(animationName != animation) return;
    }

    public void OnAnimationCommit(string animationName) {
        if(animationName != animation) return;
    }
    
    public void OnAnimationRelease(string animationName) {
        if(animationName != animation) return;
        _target.transform.position += _target.transform.forward * teleportFactor;
        GameObject.Instantiate(teleport, _target.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
    }

    public void OnAnimationEnd(string animationName) {
        if(animationName != animation) return;
        finished = true;
    }


    protected override bool ShouldEffectEnd() {
        return finished;
    }

    public override void UpdateEffect()
    {
        
    }

    public override void OnEffectEnd()
    {
        _target.OnAnimationStart -= OnAnimationStart;
        _target.OnAnimationCommit -= OnAnimationCommit;
        _target.OnAnimationRelease -= OnAnimationRelease;
        _target.OnAnimationEnd -= OnAnimationEnd;
        animator.SetBool("IsTeleporting", false);
        col.enabled = true;
        _target.SetDisabled(false);
    }

}