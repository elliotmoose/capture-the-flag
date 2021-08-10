using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    public Vector3 spawnPos {
        get {
            return _spawnPos.Value;
        }

        set { 
            _spawnPos.Value = value;
        }
    }
    
    public Quaternion spawnRot {
        get {
            return _spawnRot.Value;
        }

        set { 
            _spawnRot.Value = value;
        }
    }

    private NetworkVariableVector3 _spawnPos = new NetworkVariableVector3(new NetworkVariableSettings{
        SendTickrate=0,
        WritePermission=NetworkVariablePermission.ServerOnly
    });
    
    private NetworkVariableQuaternion _spawnRot = new NetworkVariableQuaternion(new NetworkVariableSettings{
        SendTickrate=0,
        WritePermission=NetworkVariablePermission.ServerOnly
    });
    
    public LocalPlayer localPlayer => this.GetComponent<LocalPlayer>();

    //Network Variables
    NetworkVariable<User> _user = new NetworkVariable<User>(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public string username => GetUser().username;
    private NetworkVariableFloat _catchRadius = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 4);
    
    private NetworkVariable<Team> _team = new NetworkVariable<Team>(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, Team.BLUE);

    public Team team => _team.Value;


    #region Getter Setters
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

    #endregion

    void OnEnable() {
        this._team.OnValueChanged += OnTeamChange;
    }

    void OnDisable() {
        this._team.OnValueChanged -= OnTeamChange;
    }

    public void OnTeamChange(Team oldTeam, Team newTeam) {
        Debug.Log("Player: Team set! Setting gameobject layer..");
        this.gameObject.layer = (newTeam == Team.RED ? 9 : 10);
    }

    public float SetCatchRadius(float radius) {
        return this._catchRadius.Value = radius;
    }
    public float GetCatchRadius() {
        return this._catchRadius.Value;
    }

    void Start()
    {
        
    }

    void Update() {
        
    }

    
    /// <summary>
    /// This is called locally, by the triggering client
    /// </summary>
    public void TakeNetworkEffect(EffectType effectType, ulong byClientId) {
        TakeNetworkEffectServerRpc(effectType, byClientId);
    }

    /// <summary>
    /// Client asks server to forward message to other clients
    /// asking them to self inflict an effect
    /// </summary>
    [ServerRpc(RequireOwnership=false)]
    private void TakeNetworkEffectServerRpc(EffectType effectType, ulong byClientId) {
        TakeNetworkEffectClientRpc(effectType, byClientId);
    }

    /// <summary>
    /// This is called on all clients
    /// </summary>
    /// <param name="effectType"></param>
    [ClientRpc]
    private void TakeNetworkEffectClientRpc(EffectType effectType, ulong byClientId) {
        switch (effectType)
        {
            case EffectType.Knockback:
                LocalPlayer appliedByLocalPlayer = LocalPlayer.WithClientId(byClientId);
                Vector3 direction = this.transform.position - appliedByLocalPlayer.transform.position;
                float currentDistance = direction.magnitude;
                float finalDistance = 15f;
                float timeTaken = 0.3f;
                float knockbackDistance = finalDistance - currentDistance;
                PushEffect effect = new PushEffect(localPlayer, direction, knockbackDistance, timeTaken);
                localPlayer.TakeEffect(effect);
                break;
            case EffectType.Slow:
                SlowEffect slow = new SlowEffect(localPlayer, 0.33f, 2);
                localPlayer.TakeEffect(slow);
                break;
            case EffectType.Cloned:
                GameObject trail = GameObject.Instantiate(PrefabsManager.Instance.cloneTrail, this.transform.position, Quaternion.identity);
                LocalPlayer by = LocalPlayer.WithClientId(byClientId);
                trail.GetComponent<FollowPlayer>().fromPlayer = this.gameObject;
                trail.GetComponent<FollowPlayer>().toPlayer = by.gameObject;
                break;
        }
    }
    
    public void ClientContact(ulong byClientId) {
        ContactServerRpc(byClientId);
    }

    [ServerRpc(RequireOwnership=false)]
    private void ContactServerRpc(ulong byClientId) {        
        ServerContact(LocalPlayer.WithClientId(byClientId).syncPlayer);
    }

    public void ServerContact(Player by) {
        if(!IsServer) return; //contact happens on server only
        if(by.OwnerClientId == this.OwnerClientId) return; //cannot contact self
        
        if (this.team != by.team)
        {
            if(!this.localPlayer.isCatchable) {
                Debug.Log("Can't catch, not catchable");
                return;
            }

            if(by.localPlayer.isJailed) {
                Debug.Log("Can't catch, catcher is jailed jailed");
                return;
            }
            
            if(localPlayer.isJailed) {
                Debug.Log("Can't catch, already jailed");
                return;
            }

            this.ServerImprison(by);
        }
        else //same team
        {
            if(!localPlayer.isJailed) {
                Debug.Log("Can't release, target player not jailed");
                return;
            }

            if(by.localPlayer.isJailed) {
                Debug.Log("Can't release, player trying to release is jailed");
                return;

            }

            this.ServerRelease(by);
        }
    }
    
    //onserver
    public void ServerImprison(Player by) {
        if(!IsServer) return;
        //evade
        if(localPlayer.GetType() == typeof(Rogue)) {
            bool evadeSuccess = (Random.value < Rogue.EVADE_CHANCE);
            if(evadeSuccess) {
                GameManager.Instance.TriggerOnPlayerEvade(this, by);
                return; 
            }
        }
        
        
        localPlayer.isJailed = true; //server needs to register this immediately, in order to end the round
        GameManager.Instance.TriggerOnPlayerJailed(this, by); //IMPORTANT: we trigger event to check if round ends. If this wins the round, game is no longer in progress, so no need to imprison
        if(!GameManager.Instance.roundInProgress) return;
        ImprisonClientRpc();
        // play caught sfx
        GameObject soundObj = GameObject.Instantiate(PrefabsManager.Instance.soundObject, by.transform.position, Quaternion.identity);
        SoundObject soundObject = soundObj.GetComponent<SoundObject>();
        soundObject.audioSource.clip = PrefabsManager.Instance.caughtSound;
    }

    [ClientRpc]
    private void ImprisonClientRpc() {
        if(!GameManager.Instance.roundInProgress) return;
        Debug.Log("IMPRISONED"  + gameObject.name);
        localPlayer.isJailed = true;

        // play jail sound for the jailed player
        if (localPlayer == PlayerController.LocalInstance.GetPlayer())
        {
            AudioSource playerAudio = localPlayer.GetComponent<AudioSource>();
            playerAudio.clip = PrefabsManager.Instance.jailSound;
            playerAudio.Play();
        }
        
    }

    //onserver
    public void ServerRelease(Player by) {
        if(!IsServer) return;
        localPlayer.isJailed = false; //server needs to register this immediately
        GameManager.Instance.TriggerOnPlayerFreed(this, by);
        if(!GameManager.Instance.roundInProgress) return;
        ReleaseClientRpc();
    }

    [ClientRpc]
    private void ReleaseClientRpc() {
        if(!GameManager.Instance.roundInProgress) return;
        localPlayer.isJailed = false;

        //spawn sparkle particle effect to show successful release
        GameObject.Instantiate(PrefabsManager.Instance.freedParticles, localPlayer.transform.position+new Vector3(0,1.25f), Quaternion.identity);
        // play freed sfx
        GameObject soundObj = GameObject.Instantiate(PrefabsManager.Instance.soundObject, localPlayer.transform.position, Quaternion.identity);
        SoundObject soundObject = soundObj.GetComponent<SoundObject>();
        soundObject.audioSource.clip = PrefabsManager.Instance.freedSound;
    }
}