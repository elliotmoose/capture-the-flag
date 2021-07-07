using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Flag : NetworkBehaviour
{
    public LocalPlayer capturer;
    // public Team team = Team.BLUE;
    public NetworkVariable<Team> team = new NetworkVariable<Team>(new NetworkVariableSettings{
        WritePermission=NetworkVariablePermission.ServerOnly,
        SendTickrate = 0
    });

    Renderer rendererComponent;
    // Start is called before the first frame update
    void Start()
    {
        rendererComponent = this.transform.GetChild(1).GetComponent<Renderer>();        
    }

    // Update is called once per frame
    void Update()
    {
        rendererComponent.material.color = (GetTeam() == Team.BLUE) ? new Color32(34,148,197,255) : new Color32(166,56,56,255);
        rendererComponent.material.SetColor("_EmissionColor", (GetTeam() == Team.BLUE) ? new Color32(34,148,197,255) : new Color32(191,7,5,255));
    }

    public Team GetTeam() {
        return team.Value;
    }

    void OnTriggerEnter(Collider collider) {        
        if(!IsServer) { return; }
        Player player = collider.gameObject.GetComponent<Player>();
        if(player == null) return;
        bool isDifferentTeam = (player.team != this.GetTeam());
        bool isAvailableForCapture = (capturer == null);
        if(isDifferentTeam && isAvailableForCapture) {
            GameManager.Instance.FlagCapturedBy(player);
            CapturedClientRpc(player.OwnerClientId);
        }
    }

    [ClientRpc]
    private void CapturedClientRpc(ulong byClientId) {
        LocalPlayer player = LocalPlayer.WithClientId(byClientId);
        this.transform.SetParent(player.flagSlot.transform);
        this.transform.localRotation = Quaternion.identity;
        this.transform.localPosition = Vector3.zero;
        capturer = player;
    }

    public void SetTeam(Team team) {
        if(!IsServer) { return; }
        this.team.Value = team;        
    }

    public void DispatchResetPosition() {
        if(!IsServer) return;
        ResetPositionClientRpc();
    }
    
    [ClientRpc]
    public void ResetPositionClientRpc() {
        this.capturer = null;
        this.transform.SetParent(null);
        this.transform.position = new Vector3(0,1.25f,120 * ((GetTeam() == Team.BLUE) ? 1 : -1));
        this.transform.rotation = Quaternion.identity;
    }
}
