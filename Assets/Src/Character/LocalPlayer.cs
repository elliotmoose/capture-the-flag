using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

/// <summary>
/// IMPLEMENTATION NOTE:
/// we are moving to local client state. Each player manages their own states, with respect to the following
/// 1. Movement
/// 2. Effects
/// 3. Stamina, Cooldowns, Skills
/// 4. isDisabled
/// 
/// server synced events:
/// 1. being sent to jail
/// 2. forwarding of events (i.e. player 1 slows player 2, we handover the RPC from Player to LocalPlayer)
/// </summary>
public class LocalPlayer : NetworkBehaviour
{
    //controls
    public Vector2 moveDir = Vector2.zero;
    public float faceAngle = 0;
    public bool sprinting = false;

    //player states
    public Player syncPlayer => GetComponent<Player>();
    public bool isDisabled = false;  
    public bool isJailed = false;  
    public bool isInvisToLocalPlayer {
        get {
            bool isInvis = this.GetComponent<Animator>().GetBool("IsInvisible");
            bool shouldBeInvis = (isInvis && this.team != PlayerController.LocalInstance.GetPlayer().team);
            return shouldBeInvis;
        }        
    }
    private bool _transportedToJail = false;
    private bool isMoving => (moveDir.magnitude > 0.01f && !isDisabled);
    private bool canSprint => (GetStaminaFraction() > 0 && isMoving);
    private bool isSprinting => (canSprint && sprinting);
    public bool isInEnemyTerritory {
        get {
            float z_pos = transform.position.z;
            if (this.team == Team.BLUE && z_pos <= 0) return true;
            else if (this.team == Team.RED && z_pos >= 0) return true;
            else return false;
        }
    }    

    public bool isCatchable
    {   
        get {
            bool isHoldingFlag = (GameManager.Instance.redTeamFlag.capturer == this || GameManager.Instance.blueTeamFlag.capturer == this);
            if(isHoldingFlag) return true;
            return isInEnemyTerritory;
        }
    }

    public Team team => syncPlayer.GetTeam();

    //stats
    public float moveSpeed = 15;
    float sprintMultiplier = 2.0f;
    float curStamina = 100;
    float maxStamina = 100;
    protected float staminaBurnFactor = 30;
    protected float staminaRecoveryFactor = 20;

    //skills
    public Skill catchSkill;   
    public List<Skill> skills = new List<Skill>();
    public List<Effect> effects = new List<Effect>();

    public float skill1CooldownTime = 0.0f;
    public float skill2CooldownTime = 0.0f;
    public float catchCooldownTime = 0.0f;

    //animations
    private float _animationTransitionTime = 0.15f;
    private float _curTransitionTime = 0f;

    //gameobject
    public GameObject flagSlot;
    protected Renderer[] rends;
    private Transform usernameTextTransform;
    private AudioSource playerAudio;

    #region Getter Setters    
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        this.moveSpeed = newSpeed;
    }

     public void SetMaxStamina(float maxStamina) {
        this.curStamina = curStamina/maxStamina*maxStamina;
        this.maxStamina = maxStamina;
    }
    
    public float GetStaminaFraction() {
        return curStamina/maxStamina;
    }

    #endregion
    void Start()
    {
        this.rends = this.GetComponentsInChildren<Renderer>();
        this.playerAudio = this.GetComponent<AudioSource>();
        this.flagSlot = this.transform.Find("model/body/FlagSlot").gameObject;
        if (!flagSlot)
        {
            Debug.LogError("This player has no flag slot");
        }

        SpawnUsername();
    }

    private bool _rendererSetupComplete = false;

    /// <summary>
    /// sets up render to prepare for invisibility, depending on whether this player is on the same team or not
    /// </summary>
    void ClientsSetupRendererIfNeeded() {
        if(!_rendererSetupComplete && PlayerController.LocalInstance != null && PlayerController.LocalInstance.GetPlayer() != null && !syncPlayer.GetUser().IsNull() && !PlayerController.LocalInstance.GetUser().IsNull()) {
            float alphaValue = (this.team == PlayerController.LocalInstance.GetPlayer().team) ? 0.3f : 0;
            //SET RENDERER FOR INVISIBILTY
            Renderer[] rends = this.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < this.rends.Length; i++)
            {
                Renderer rend = rends[i];
                foreach(Material mat in rend.materials) {
                    // Debug.LogError($"Setting {this.syncPlayer.username} ({this.team}) material to {alphaValue} as this client is viewing from {PlayerController.LocalInstance.GetPlayer().team} team");
                    mat.SetFloat("_alphaValue", alphaValue);
                }
            }       

            //SET RENDERER FOR CATCH
            this.transform.Find("Catch/CatchField").GetComponent<Renderer>().material.SetColor("_emission", this.team == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed);

            _rendererSetupComplete = true;
            // Debug.Log($"Renderer Initialised for player: {syncPlayer.GetUser().username}");
        }
    }

    private void ClientsUpdateUsername() {
        usernameTextTransform.gameObject.SetActive(!isInvisToLocalPlayer); //for invis
        TMPro.TextMeshPro textMesh = usernameTextTransform.GetComponent<TMPro.TextMeshPro>();
        textMesh.text = syncPlayer.GetUser().username;
        textMesh.color = this.team == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed;
        usernameTextTransform.rotation = Quaternion.LookRotation( usernameTextTransform.position - Camera.main.transform.position );
    }  

    private void SpawnUsername()
    {
        usernameTextTransform = GameObject.Instantiate(PrefabsManager.Instance.playerUsername, this.transform).transform;
        usernameTextTransform.transform.localPosition = new Vector3(0, 3, 0);
        usernameTextTransform.transform.localRotation = Quaternion.identity;
    }

    private void SetAnimationsSmooth(bool isMoving, bool isSprinting) {
        if(!IsOwner) { return; }

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


    
    void FixedUpdate() {
        if(!IsOwner) return;
        FixedUpdateEffects();
        FixedUpdateMovement();
    }   

    void FixedUpdateMovement() {
        if(!IsOwner) return;
        if(!GameManager.Instance.roundInProgress) { return; }
        if(isMoving) {                
            float moveDirAngle = Mathf.Atan2(moveDir.x, moveDir.y) * Mathf.Rad2Deg + faceAngle;
            Vector3 positionDelta = Quaternion.Euler(0, moveDirAngle, 0) * Vector3.forward;

            if(isSprinting) {
                // transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed * sprintMultiplier;
                GetComponent<CharacterController>().Move(positionDelta.normalized * moveSpeed * sprintMultiplier * Time.fixedDeltaTime);
            }
            else {
                GetComponent<CharacterController>().Move(positionDelta.normalized * moveSpeed * Time.fixedDeltaTime);
                // transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed;
            }
            
        }      
    }

    void Update()
    {        
        if(!GameManager.Instance.gameInProgress) { return; }
        ClientsSetupRendererIfNeeded();        
        ClientsUpdateUsername();
        UpdateSfx();
        
        if(!IsOwner) return;
        
        this.transform.rotation = Quaternion.Euler(0, faceAngle, 0);
        if(!GameManager.Instance.roundInProgress) { return; }
        
        SetAnimationsSmooth(isMoving, isSprinting);
        UpdateStamina();
        UpdateCooldowns(); //update cooldowns
        UpdateEffects(); // update skill effects applied to player
    }

    void LateUpdate()
    {
        UpdatePositionIfJailed();
        this.transform.position = new Vector3(this.transform.position.x, 0, this.transform.position.z);
    }

    void UpdateSfx() {
        AudioSource source = this.transform.Find("sfx_movement").GetComponent<AudioSource>();
        Animator animator = GetComponent<Animator>();
        //[0, 1]
        float movement = Mathf.Max(Mathf.Abs(animator.GetFloat("HorMovement")), Mathf.Abs(animator.GetFloat("VertMovement")));        
        source.pitch = Mathf.Lerp(1.73f, 2, movement);
        float minVolume = 0.02f;
        float maxVolume = 0.04f;
        source.volume = Mathf.Min(0.5f, movement) * 2 * (maxVolume-minVolume) + minVolume;
    }

    private void UpdatePositionIfJailed() {
        if(!isJailed) _transportedToJail = false; //reset. This is for the purpose of telling smoothsync to teleport to center
        if(IsOwner && isJailed) {
            Jail jail = JailManager.Instance.JailForPlayerOfTeam(team);
            Vector3 jailCenter = jail.transform.position;
            if(!_transportedToJail) {
                Debug.Log("JAILED! (Teleport Triggered)");
                this.transform.position = jailCenter;
                this.GetComponent<Smooth.SmoothSyncMLAPI>().teleportOwnedObjectFromOwner();
                _transportedToJail = true;
            }
            else {
                Vector3 jailToPlayer = (this.transform.position-jailCenter);
                if(jailToPlayer.magnitude > jail.jailSize) {
                    Vector3 newPos = jailCenter + jailToPlayer.normalized*jail.jailSize;
                    this.transform.position = new Vector3(newPos.x, this.transform.position.y, newPos.z);
                }
            }
        }
    }

    public void SetDisabled(bool disabled)
    {
        this.isDisabled = disabled;
    }


    public void TakeEffect(Effect effect)
    {
        if(!IsOwner) return;
        if(!GameManager.Instance.roundInProgress) { return; }

        Effect existingEffect = this.effects.Find((thisEffect) =>
        {
            return thisEffect.name == effect.name;
        });

        // if same effect already exists on player, replace it with newer one
        if (existingEffect != null)
        {
            // existingEffect.OnEffectEnd();
            // this.effects.Remove(existingEffect);
            // this.effects.Add(effect);
            Debug.Log(effect.name + " cannot be stacked!!!!");
            return;
        }
        effect.OnEffectApplied();
        this.effects.Add(effect);
        Debug.Log(effect.name + " effect taken");
        
    }

    void UpdateStamina()
    {
        //expend
        if (isMoving && isSprinting)
        {
            curStamina = Mathf.Max(0, curStamina - Time.deltaTime * staminaBurnFactor);
        }

        //recover
        if (!sprinting && !isDisabled)
        {
            curStamina = Mathf.Clamp(curStamina + Time.deltaTime * staminaRecoveryFactor, 0, maxStamina);
        }
    }

    // either increase or decrease stamina
    public void buffStamina(float x)
    {
        if (x > 0)
        {
            curStamina = Mathf.Clamp(curStamina + Time.deltaTime * staminaRecoveryFactor * x, 0, maxStamina);
        } else
        {
            curStamina = Mathf.Max(0, curStamina - Time.deltaTime * staminaBurnFactor * -x);
        }
        
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

    public Effect GetEffectWithName(string effectName) {
        foreach(Effect effect in effects) {
            if(effect.name == effectName) return effect;
        }

        return null;
    }

    public bool HasEffect(string effectName) {
        return GetEffectWithName(effectName) != null;
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

    public void PassFlag() {
        if(!GameManager.Instance.roundInProgress) return;
        Flag flag = GameManager.Instance.FlagForPlayer(this);
        if(flag != null) flag.ClientHandoverFlag();
    }

    public void CastSkillAtIndex(int index)
    {
        if (!GameManager.Instance.roundInProgress) { return; }

        if (index >= skills.Count)
        {
            Debug.LogWarning($"Skill index out of range: {index}");
            return;
        }

        Skill skill = skills[index];

        if (index == 0)
        {
            if (skill1CooldownTime < 0)
            {
                skill.UseSkill(this);
                skill1CooldownTime = skill.cooldown;
            }
            else
            {
                // Debug.Log($"Skill 1 remainding cooldown: {skill1CooldownTime}");
            }
        }

        if (index == 1)
        {
            if (skill2CooldownTime < 0)
            {
                skill.UseSkill(this);
                skill2CooldownTime = skill.cooldown;
            }
            else
            {
                // Debug.Log($"Skill 2 remainding cooldown: {skill2CooldownTime}");
            }
        }
    }

    void UpdateCooldowns() {
        skill1CooldownTime -= Time.deltaTime;
        skill2CooldownTime -= Time.deltaTime;
        catchCooldownTime -= Time.deltaTime;
    }
    
    #region Animation Events
    public delegate void PlayerAnimationEvent(string animationName);
    public PlayerAnimationEvent OnAnimationStart;
    public PlayerAnimationEvent OnAnimationCommit;
    public PlayerAnimationEvent OnAnimationRelease;
    public PlayerAnimationEvent OnAnimationEnd;
    public PlayerAnimationEvent OnAnimationSound;

    public void AnimationStart(string animationName) {        
        if(animationName == "Teleport") SpawnTeleportStartParticle();
        Debug.Log($"catch radius: {syncPlayer.GetCatchRadius()}");
        if (animationName == "Catch") this.transform.Find("Catch").localScale = Vector3.one * syncPlayer.GetCatchRadius()*2; //we need this to be here so that it is replicated across all
        if (animationName == "Smoke") SpawnSmokeParticle();
        if (animationName == "Knockback") SpawnKnockbackParticle();
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

    public void AnimationSound(string animationName)
    {
        if (animationName == "Reach") PlayReachSFX();
        if (animationName == "Smoke") PlaySmokeSFX();
        if (animationName == "Buff") PlayBuffSFX();
        if (animationName == "Boost") PlayBoostSFX();
        if (animationName == "Knockback") PlaySlamSFX();
        if (animationName == "Teleport") PlayTeleportSFX();
        if (animationName == "Slow") PlaySlowSFX();
        if (animationName == "Invis") PlayInvisSFX();
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

    void SpawnKnockbackParticle()
    {
        GameObject.Instantiate(PrefabsManager.Instance.knockbackField, this.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
    }

    #endregion

    #region skill sfx
    void PlayReachSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.laserSound;
        playerAudio.Play();
    }

    void PlayBuffSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.buffSound;
        playerAudio.Play();
    }

    void PlaySmokeSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.smokeSound;
        playerAudio.Play();
    }

    void PlayBoostSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.boostSound;
        playerAudio.Play();
    }

    void PlaySlamSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.slamSound;
        playerAudio.Play();
    }

    void PlayTeleportSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.teleportSound;
        playerAudio.Play();
    }

    void PlaySlowSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.electricitySound;
        playerAudio.Play();
    }

    void PlayInvisSFX()
    {
        playerAudio.clip = PrefabsManager.Instance.invisSound;
        playerAudio.Play();
    }


    #endregion

    #region Local-Network Interface

    public void TakeNetworkEffect(EffectType effectType, ulong byClientId) {
        syncPlayer.TakeNetworkEffect(effectType, byClientId);
    }    
    
    public void ClientContact(LocalPlayer by) {
        syncPlayer.ClientContact(by.OwnerClientId);
    }

    #endregion

    #region Network-Local Interface
    
    public void ResetForRound() {
        Debug.Log($"Player reset: {this.gameObject.name}");
        this.isJailed = false;
        this.transform.position = syncPlayer.spawnPos;
        this.transform.rotation = syncPlayer.spawnRot;

        if(!IsOwner) return; 
        
        //reset position
        // this.GetComponent<Smooth.SmoothSyncMLAPI>().teleportOwnedObjectFromOwner();
        
        //reset stats
        this.skill1CooldownTime = 0;
        this.skill2CooldownTime = 0;
        this.catchCooldownTime = 0;
        this.curStamina = this.maxStamina;

        //reset effects
        for(int i=this.effects.Count-1; i>=0 && i < this.effects.Count; i--)
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

    public bool hasReset {
        get {
            bool jailReset = !isJailed;
            bool staminaReset = (curStamina == maxStamina);
            bool transformReset = (Vector3.Distance(this.transform.position, syncPlayer.spawnPos) < 0.01f); 
            bool effectsReset = this.effects.Count == 0;
            return jailReset && staminaReset && transformReset && effectsReset;
        }
    }

    #endregion

    public static LocalPlayer WithClientId(ulong clientId) {
        GameObject[] playerGOs = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject playerGO in playerGOs) {
            LocalPlayer player = playerGO.GetComponent<LocalPlayer>();
            if(player.OwnerClientId == clientId) {
                return player;
            }
        }

        return null;
    }

    public static List<LocalPlayer> AllPlayers() {
        GameObject[] playerGOs = GameObject.FindGameObjectsWithTag("Player");
        List<LocalPlayer> players = new List<LocalPlayer>();

        foreach(GameObject playerGO in playerGOs) {
            LocalPlayer localPlayer = playerGO.GetComponent<LocalPlayer>();
            players.Add(localPlayer);
        }

        return players;
    }
    
    public static List<LocalPlayer> PlayersFromTeam(Team team) {
        GameObject[] playerGOs = GameObject.FindGameObjectsWithTag("Player");
        List<LocalPlayer> players = new List<LocalPlayer>();

        foreach(GameObject playerGO in playerGOs) {
            LocalPlayer localPlayer = playerGO.GetComponent<LocalPlayer>();
            if(team == localPlayer.team) {
                players.Add(localPlayer);
            }
        }

        return players;
    }

}