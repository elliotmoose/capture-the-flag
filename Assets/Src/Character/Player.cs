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
        
    protected float moveSpeed = 15;
    protected float staminaBurnFactor = 30;
    protected float staminaRecoveryFactor = 20;
    protected bool isDisabled = false;

    
    float sprintMultiplier = 2.4f;
    public bool sprinting = false;

    private float _animationTransitionTime = 0.15f;
    private float _curTransitionTime = 0f;

    private Transform usernameTextTransform;

    //Network Variables
    NetworkVariable<User> _user = new NetworkVariable<User>(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    private NetworkVariableFloat _catchRadius = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 8);
    
    private NetworkVariableFloat _curStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 20,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);
    
    private NetworkVariableFloat _maxStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);
    
    private NetworkVariable<Team> _team = new NetworkVariable<Team>(new NetworkVariableSettings{
        SendTickrate = 0,
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

    public User GetUser() {
        return _user.Value;
    }

    public void SetUser(User user) {
        this._user.Value = user;
    }

    public float SetCatchRadius(float radius) {
        return this._catchRadius.Value = radius;
    }
    public float GetCatchRadius() {
        return this._catchRadius.Value;
    }

    public void SetMaxStamina(float maxStamina) {
        this._curStamina.Value = _curStamina.Value/_maxStamina.Value*maxStamina;
        this._maxStamina.Value = maxStamina;
    }
    
    public float GetStaminaFraction() {
        return _curStamina.Value/_maxStamina.Value;
    }

    private void SpawnUsername() {
        usernameTextTransform = GameObject.Instantiate(PrefabsManager.Instance.playerUsername, this.transform).transform;
        usernameTextTransform.transform.localPosition = new Vector3(0,3,0);
        usernameTextTransform.transform.localRotation = Quaternion.identity;
    }

    private void UpdateUsername() {
        bool isInvis = this.GetComponent<Animator>().GetBool("IsInvisible");
        bool shouldShowUsername = (!isInvis || this.GetTeam() == PlayerController.LocalInstance.GetPlayer().GetTeam());
        usernameTextTransform.gameObject.SetActive(shouldShowUsername); //for invis
        TMPro.TextMeshPro textMesh = usernameTextTransform.GetComponent<TMPro.TextMeshPro>();
        textMesh.text = GetUser().username;
        textMesh.color = GetUser().team == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed;
        usernameTextTransform.rotation = Quaternion.LookRotation( usernameTextTransform.position - Camera.main.transform.position );
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

        SpawnUsername();        
    }

    private bool _rendererSetupComplete = false;

    void SetupRendererIfNeeded() {
        if(!_rendererSetupComplete && PlayerController.LocalInstance != null && PlayerController.LocalInstance.GetPlayer() != null && !GetUser().IsNull() && !PlayerController.LocalInstance.GetUser().IsNull()) {
            //SET RENDERER FOR INVISIBILTY
            Renderer[] rends = this.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                if (this.GetTeam() == PlayerController.LocalInstance.GetPlayer().GetTeam())
                {
                    // if same team, appear transparent
                    rend.material.SetFloat("_alphaValue", 0.3f);
                }
                else
                {
                    // if enemy team, appear invisible
                    rend.material.SetFloat("_alphaValue", 0f);
                }
            }       

            //SET RENDERER FOR CATCH
            this.transform.Find("Catch/CatchField").GetComponent<Renderer>().material.SetColor("_emission", this.GetTeam() == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed);

            _rendererSetupComplete = true;
            Debug.Log($"Renderer Initialised for player: {GetUser().username}");
        }
    }

    void FixedUpdate() {
        if(!IsServer) return;
        FixedUpdateEffects();
    }

    void Update()
    {
        SetupRendererIfNeeded();
        UpdateUsername();

        if(!IsServer) { return; }
        if(!GameManager.Instance.roundInProgress) { return; }

        bool isMoving = (moveDir.magnitude > 0.01f && !isDisabled);
        bool canSprint = (_curStamina.Value > 0 && isMoving);
        bool isSprinting = (canSprint && sprinting);
        this.transform.rotation = Quaternion.Euler(0, faceAngle, 0);
        //Debug.Log(curStamina);

        if(isMoving) {                
            float moveDirAngle = Mathf.Atan2(moveDir.x, moveDir.y) * Mathf.Rad2Deg + faceAngle;
            Vector3 positionDelta = Quaternion.Euler(0, moveDirAngle, 0) * Vector3.forward;

            if(isSprinting) {
                // transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed * sprintMultiplier;
                GetComponent<CharacterController>().SimpleMove(positionDelta.normalized * moveSpeed * sprintMultiplier);
                _curStamina.Value = Mathf.Max(0, _curStamina.Value - Time.deltaTime * staminaBurnFactor);
            }
            else {
                GetComponent<CharacterController>().SimpleMove(positionDelta.normalized * moveSpeed);
                // transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed;
            }

            // transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
        
        //recover
        if(!sprinting && !isDisabled) {
            _curStamina.Value = Mathf.Clamp(_curStamina.Value + Time.deltaTime * staminaRecoveryFactor, 0, _maxStamina.Value);
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
            // Debug.Log($"Catch remainding cooldown: {catchCooldownTime}");
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
                // Debug.Log($"Skill 1 remainding cooldown: {skill1CooldownTime}");
            }
        }

        if(index == 1) {
            if(skill2CooldownTime < 0) {
                skill.UseSkill(this);
                skill2CooldownTime = skill.cooldown;
            }
            else {
                // Debug.Log($"Skill 2 remainding cooldown: {skill2CooldownTime}");
            }
        }
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
    
    public void FixedUpdateEffects()
    {        
        if(!GameManager.Instance.roundInProgress) { return; }

        for(int i=this.effects.Count-1; i>=0 && i<this.effects.Count; i++)
        {
            Effect effect = this.effects[i];
            effect.FixedUpdate();
            
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

    public bool IsInEnemyTerritory(){
        float z_pos = transform.position.z;
        if (this.GetTeam() == Team.BLUE && z_pos <= 0) return true;
        else if (this.GetTeam() == Team.RED && z_pos >= 0) return true;
        else return false;
    }

    public bool IsCatchable()
    {   
        bool isHoldingFlag = (GameManager.Instance.redTeamFlag.capturer == this || GameManager.Instance.blueTeamFlag.capturer == this);
        if(isHoldingFlag) return true;        
        return IsInEnemyTerritory();
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
        this._curStamina.Value = _maxStamina.Value;

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

    #region Animation Events
    public delegate void PlayerAnimationEvent(string animationName);
    public PlayerAnimationEvent OnAnimationStart;
    public PlayerAnimationEvent OnAnimationCommit;
    public PlayerAnimationEvent OnAnimationRelease;
    public PlayerAnimationEvent OnAnimationEnd;

    public void AnimationStart(string animationName) {
        if(animationName == "Catch") this.transform.Find("Catch").localScale = Vector3.one * GetCatchRadius();
        if(animationName == "Teleport") SpawnTeleportStartParticle();
        if (animationName == "Smoke") SpawnSmokeParticle();
        if (OnAnimationStart!=null) OnAnimationStart(animationName);
    }

    public void AnimationCommit(string animationName) {
        if (OnAnimationCommit!=null) OnAnimationCommit(animationName);
    }
    
    public void AnimationRelease(string animationName) {
        if (OnAnimationRelease!=null) OnAnimationRelease(animationName);
    }
    
    public void AnimationEnd(string animationName) {
        if(animationName == "Teleport") SpawnTeleportEndParticle();
        if (OnAnimationEnd!=null) OnAnimationEnd(animationName);
    }
    #endregion

    #region skill particle effect sync
    void SpawnTeleportStartParticle() {
        GameObject.Instantiate(PrefabsManager.Instance.teleportField, this.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
    }
    
    void SpawnTeleportEndParticle() {
        GameObject.Instantiate(PrefabsManager.Instance.teleportField, this.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
    }

    void SpawnSmokeParticle()
    {
        GameObject.Instantiate(PrefabsManager.Instance.smoke, this.transform.position, Quaternion.identity);
    }

    #endregion
}