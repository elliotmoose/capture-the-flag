using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    protected Skill catchSkill;   
    public List<Skill> skills = new List<Skill>();
    public List<Effect> effects = new List<Effect>();
    protected float cdTimer1 = 0.0f;
    protected float cdTimer2 = 0.0f;
    protected float cdTimerCD = 0.0f;
    
    //movement controls
    public GameObject flagSlot;
    public Vector2 moveDir = Vector2.zero;
    public float faceAngle = 0;
    protected Renderer[] rends;

    protected float catchRadius = 6;
    protected float moveSpeed = 18;
    protected float staminaBurnFactor = 25;
    protected float staminaRecoveryFactor = 30;
    protected bool isDisabled = false;

    

    float sprintMultiplier = 2.4f;
    public bool sprinting = false;
    public Team team = Team.BLUE;

    public NetworkVariableULong ownerClientId = new NetworkVariableULong(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public void SetMoveSpeed(float newSpeed)
    {
        this.moveSpeed = newSpeed;
    }

    public float GetCatchRadius()
    {
        return catchRadius;
    }

    public void SetCatchRadius(float radius)
    {
        this.catchRadius = radius;
    }

    void Start()
    {
        this.rends = this.GetComponentsInChildren<Renderer>();     
        this.flagSlot = this.transform.Find("model/body/FlagSlot").gameObject;
        if(!flagSlot) {
            Debug.LogError("This player has no flag slot");
        }   
    }

    // Update is called once per frame
    public NetworkVariableFloat curStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 20,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);
    
    public NetworkVariableFloat maxStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);

    private float _animationTransitionTime = 0.15f;
    private float _curTransitionTime = 0f;

    void Update()
    {
        if(!IsServer) { return; }

        bool isMoving = (moveDir.magnitude > 0.01f && !isDisabled);
        bool canSprint = (curStamina.Value > 0 && isMoving);
        bool isSprinting = (canSprint && sprinting);
        this.transform.rotation = Quaternion.Euler(0, faceAngle, 0);
        //Debug.Log(curStamina);

        if(isMoving) {                
            float moveDirAngle = Mathf.Atan2(moveDir.x, moveDir.y) * Mathf.Rad2Deg + faceAngle;
            Vector3 positionDelta = Quaternion.Euler(0, moveDirAngle, 0) * Vector3.forward;

            if(isSprinting) {
                transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed * sprintMultiplier;
                curStamina.Value = Mathf.Max(0, curStamina.Value - Time.deltaTime * staminaBurnFactor);
            }
            else {
                transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed;
            }
        }
        
        //recover
        if(!sprinting && !isDisabled) {
            curStamina.Value = Mathf.Clamp(curStamina.Value + Time.deltaTime * staminaRecoveryFactor, 0, maxStamina.Value);
        }

        SetAnimationsSmooth(isMoving, isSprinting);

        UpdateCooldowns(); //update cooldowns
        UpdateEffects(); // update skill effects applied to player
    }

    public void Catch()
    {
        if(cdTimerCD < 0)
        {
            catchSkill.UseSkill(this);
            cdTimerCD = catchSkill.cooldown;
        }
        else
        {
            Debug.Log($"Catch remainding cooldown: {cdTimerCD}");
        }

    }

    public void CastSkillAtIndex(int index) {
        if(index >= skills.Count) {
            Debug.LogWarning($"Skill index out of range: {index}");
            return;
        }

        Skill skill = skills[index];

        if(index == 0) {
            if(cdTimer1 < 0) {
                skill.UseSkill(this);
                cdTimer1 = skill.cooldown;
            }
            else {
                Debug.Log($"Skill 1 remainding cooldown: {cdTimer1}");
            }
        }

        if(index == 1) {
            if(cdTimer2 < 0) {
                skill.UseSkill(this);
                cdTimer2 = skill.cooldown;
            }
            else {
                Debug.Log($"Skill 2 remainding cooldown: {cdTimer2}");
            }
        }
    }

    void LateUpdate() {
        //TODO: face based on facedir
        // if(NetworkManager.LocalClientId == ownerClientId.Value) {
        //     transform.rotation = Quaternion.Euler(transform.rotation.x, Camera.main.transform.eulerAngles.y, transform.rotation.z);
        // }
    }

    // receive effect
    public void TakeEffect(Effect effect)
    {
        Effect existingEffect = this.effects.Find((thisEffect) =>
        {
            return thisEffect.name == effect.name;
        });

        // if same effect already exists on player, replace it with newer one
        if (existingEffect != null)
        {
            this.effects.Remove(existingEffect);
            this.effects.Add(effect);
            return;
        }
        effect.OnEffectApplied();
        this.effects.Add(effect);
        Debug.Log(effect.name + " effect taken");
        
    }

    public void UpdateEffects()
    {        
        for(int i=0; i< this.effects.Count; i++)
        {
            Effect effect = this.effects[i];
            effect.Update();
            
            if (effect.effectEnded)
            {
                effects.Remove(effect);
            } 
        }
    }

    void UpdateCooldowns() {
        cdTimer1 -= Time.deltaTime;
        cdTimer2 -= Time.deltaTime;
        cdTimerCD -= Time.deltaTime;
    }
    
    public void SetDisabled(bool disabled)
    {
        this.isDisabled = disabled;
    }

    
    public bool IsCatchable()
    {
        float z_pos = transform.position.z;
        if (this.team == Team.BLUE && z_pos <= 0)
        {
            return true;
        }
        else if (this.team == Team.RED && z_pos >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetAnimationsSmooth(bool isMoving, bool isSprinting) {
        if(!IsServer) { return; }

        Vector2 targetMoveDir = moveDir/(isSprinting ? 1 : 2);
        //animation smoohting
        Animator animator = GetComponent<Animator>();
        Vector2 curMoveDir = new Vector2(animator.GetFloat("HorMovement"), animator.GetFloat("VertMovement")); //current movedir state

        if(curMoveDir != targetMoveDir) {
            // _lastMoveDir = curMoveDir;
            _curTransitionTime += Time.deltaTime;
            Vector2 newSmoothMoveDir = Vector2.Lerp(curMoveDir, targetMoveDir, _curTransitionTime/_animationTransitionTime);
            animator.SetFloat("HorMovement", newSmoothMoveDir.x);
            animator.SetFloat("VertMovement", newSmoothMoveDir.y);            
        }
        else {
            _curTransitionTime = 0;
        }           
        
        animator.SetBool("IsMoving", isMoving);
    }
    
    public void ResetForRound() {
        this.curStamina.Value = maxStamina.Value;
        this.transform.position = Vector3.zero; //todo: make spawn point
    }
}