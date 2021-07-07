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

    private NetworkVariableBool _isJailed = new NetworkVariableBool(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public bool isJailed => _isJailed.Value;

    private NetworkVariableFloat _catchRadius = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 8);
    
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

    public void DispatchResetForRound() {
        ResetForRoundClientRpc();
        _isJailed.Value = false;
    }

    [ClientRpc]
    private void ResetForRoundClientRpc() {
        localPlayer.ResetForRound();        
    }
    
    public void ClientContact(ulong byClientId) {
        ContactServerRpc(byClientId);
    }

    [ServerRpc(RequireOwnership=false)]
    public void ContactServerRpc(ulong byClientId) {        
        ServerContact(LocalPlayer.WithClientId(byClientId).syncPlayer);
    }

    public void ServerContact(Player by) {
        if(!IsServer) return; //contact happens on server only
        if(by.OwnerClientId == this.OwnerClientId) return; //cannot contact self

        if (this.team != by.team)
        {
            if (localPlayer.isCatchable)
            {
                this.ServerImprison(by);
            }
        }
        else //same team
        {
            this.ServerRelease(by);
        }
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
                float finalDistance = 18f;
                float timeTaken = 0.25f;
                float knockbackDistance = finalDistance - currentDistance;
                PushEffect effect = new PushEffect(localPlayer, direction, knockbackDistance, timeTaken);
                localPlayer.TakeEffect(effect);
                break;
            case EffectType.Slow:
                SlowEffect slow = new SlowEffect(localPlayer, 3, 2);
                localPlayer.TakeEffect(slow);
                break;
        }
    }

    //onserver
    public void ServerImprison(Player by) {
        if(!IsServer) return;
        if(!isJailed) {
            _isJailed.Value = true;
            GameManager.Instance.TriggerOnPlayerJailed(this, by);
        }
    }

    //onserver
    public void ServerRelease(Player by) {
        if(!IsServer) return;
        if(isJailed) {
            _isJailed.Value = false;
            GameManager.Instance.TriggerOnPlayerFreed(this, by);
        }
    }
}