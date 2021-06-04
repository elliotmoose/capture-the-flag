using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    public Vector3 spawnPos;
    public Quaternion spawnDir;
    
    public Skill catchSkill;   
    public List<Skill> skills = new List<Skill>();
    public List<Effect> effects = new List<Effect>();
    
    //movement controls
    public GameObject flagSlot;
    public Vector2 moveDir = Vector2.zero;
    public float faceAngle = 0;
    protected Renderer[] rends;

    protected float catchRadius = 5;
    protected float moveSpeed = 12;
    protected float staminaBurnFactor = 30;
    protected float staminaRecoveryFactor = 35;
    protected bool isDisabled = false;

    //skill variables
    protected float invisAlpha = 0.3f;
    
    float sprintMultiplier = 2.4f;
    public bool sprinting = false;

    private float _animationTransitionTime = 0.15f;
    private float _curTransitionTime = 0f;

    //Network Variables
    public NetworkVariableULong ownerClientId = new NetworkVariableULong(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public NetworkVariableFloat curStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 20,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);
    
    public NetworkVariableFloat maxStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);


    NetworkVariableBool isInvis = new NetworkVariableBool(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, false);
    // protected bool isInvis = false;
    private NetworkVariable<Team> _team = new NetworkVariable<Team>(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, Team.BLUE);
    
    
    public float skill1CooldownTime = 0.0f;
    public float skill2CooldownTime = 0.0f;
    public float catchCooldownTime = 0.0f;

    public Team GetTeam() {
        return _team.Value;
    }

    public void SetTeam(Team team) {
        this._team.Value = team;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public void SetMoveSpeed(float newSpeed)
    {
        this.moveSpeed = newSpeed;
    }

    void Start()
    {
        this.rends = this.GetComponentsInChildren<Renderer>();     
        this.flagSlot = this.transform.Find("model/body/FlagSlot").gameObject;
        if(!flagSlot) {
            Debug.LogError("This player has no flag slot");
        }      
    }

    void Update()
    {
        UpdateInvisRenderer();

        if(!IsServer) { return; }
        if(!GameManager.Instance.roundInProgress) { return; }

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
        if(!GameManager.Instance.roundInProgress) { return; }

        if(catchCooldownTime < 0)
        {
            catchSkill.UseSkill(this);
            catchCooldownTime = catchSkill.cooldown;
        }
        else
        {
            Debug.Log($"Catch remainding cooldown: {catchCooldownTime}");
        }

    }

    public void CastSkillAtIndex(int index) {
        if(!GameManager.Instance.roundInProgress) { return; }

        if(index >= skills.Count) {
            Debug.LogWarning($"Skill index out of range: {index}");
            return;
        }

        Skill skill = skills[index];

        if(index == 0) {
            if(skill1CooldownTime < 0) {
                skill.UseSkill(this);
                skill1CooldownTime = skill.cooldown;
            }
            else {
                Debug.Log($"Skill 1 remainding cooldown: {skill1CooldownTime}");
            }
        }

        if(index == 1) {
            if(skill2CooldownTime < 0) {
                skill.UseSkill(this);
                skill2CooldownTime = skill.cooldown;
            }
            else {
                Debug.Log($"Skill 2 remainding cooldown: {skill2CooldownTime}");
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
        if(!GameManager.Instance.roundInProgress) { return; }

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
        if(!GameManager.Instance.roundInProgress) { return; }

        for(int i=this.effects.Count-1; i>=0 && i<this.effects.Count; i++)
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
        skill1CooldownTime -= Time.deltaTime;
        skill2CooldownTime -= Time.deltaTime;
        catchCooldownTime -= Time.deltaTime;
    }
    
    public void SetDisabled(bool disabled)
    {
        this.isDisabled = disabled;
    }

    public void SetInvis(bool invis)
    {
        this.isInvis.Value = invis;
    }
    
    public void UpdateInvisRenderer() {
        // check if player is invisible
        if (isInvis.Value)
        {
            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                if (this.GetTeam() == PlayerController.LocalInstance.GetPlayer().GetTeam())
                {
                    // if same team, appear transparent
                    rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, invisAlpha);
                }
                else
                {
                    // if enemy team, appear invisible
                    rend.enabled = false;
                }
            }
        }
        else
        {
            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];

                // revert invisible effect
                if (this.GetTeam() == PlayerController.LocalInstance.GetPlayer().GetTeam())
                {
                    rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1.0f);
                }
                else
                {
                    rend.enabled = true;
                }
            }
        }
    }

    public bool IsCatchable()
    {
        float z_pos = transform.position.z;
        if (this.GetTeam() == Team.BLUE && z_pos <= 0)
        {
            return true;
        }
        else if (this.GetTeam() == Team.RED && z_pos >= 0)
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
        //reset jail
        GameManager.Instance.ResetJail();
        
        //reset position
        this.transform.position = spawnPos;
        this.transform.rotation = spawnDir; 
        
        //reset stats
        this.curStamina.Value = maxStamina.Value;

        //reset effects
        for(int i=this.effects.Count-1; i>=0 && i < this.effects.Count; i++)
        {
            Effect effect = this.effects[i];
            effect.OnEffectEnd();
        }
        this.effects.Clear(); 

        //reset animation 
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("HorMovement", 0);
        animator.SetFloat("VertMovement", 0);      
        animator.SetBool("IsMoving", false);         
    }
}