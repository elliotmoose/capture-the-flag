using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

// Teleport skill
public class Teleport : Skill
{
    private float teleportFactor = 30.0f;

    public Teleport()
    {
        cooldown = 7.0f;
        name = "Teleport";
        this.icon = PrefabsManager.Instance.teleportIcon;
    }

    public override void UseSkill(LocalPlayer player)
    {
        Debug.Log(name + " skill is used");

        TeleportEffect effect = new TeleportEffect(player, teleportFactor);
        player.TakeEffect(effect);
    }
}

public class TeleportEffect : Effect
{
    private float teleportFactor;
    private Collider col;
    private Renderer[] rends;
    private Animator animator;
    private string animation = "Teleport";
    private bool finished = false;

    public TeleportEffect(LocalPlayer _target, float teleportFactor) : base(_target)
    {
        this.duration = 1.2f;
        this.teleportFactor = teleportFactor;
        this.name = "TELEPORT_EFFECT";        
        this.col = _target.GetComponent<Collider>();
        this.rends = _target.GetComponentsInChildren<Renderer>();
        this.animator = _target.GetComponent<Animator>();
    }

    public override void OnEffectApplied()
    {        
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
    

    private Vector3 GetTeleportDestination() {
        int testResolution = 10;
        Vector3 start = _target.transform.position;
        Vector3 end = _target.transform.position + _target.transform.forward * teleportFactor;        
        CharacterController characterController = _target.GetComponent<CharacterController>();
        Vector3 targetPos = this._target.transform.position;
        for(int i=testResolution; i>=0; i--) {
            targetPos = Vector3.Lerp(start, end, (float)i/(float)testResolution);
            Vector3 vertOffset = Vector3.up*(characterController.radius + characterController.height/2);            
            string ownZoneColliderLayer = (_target.team == Team.BLUE ? "BlueZoneCollider" : "RedZoneCollider");
            Collider[] hits = Physics.OverlapCapsule(targetPos+vertOffset, targetPos+vertOffset, characterController.radius, LayerMask.GetMask(ownZoneColliderLayer, "Terrain"));
            bool validDestination = (hits.Length == 0);
            if(validDestination) {
                // Debug.Log("Found a no collision spot!!");
                break;
            }
        }
        
        return targetPos;
    }
    public void OnAnimationRelease(string animationName) {
        if(animationName != animation) return;
        _target.transform.position = GetTeleportDestination();
        _target.GetComponent<Smooth.SmoothSyncMLAPI>().teleportOwnedObjectFromOwner();
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