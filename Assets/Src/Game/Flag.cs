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
    // public NetworkVariable<Team> team = new NetworkVariable<Team>(new NetworkVariableSettings{
    //     WritePermission=NetworkVariablePermission.ServerOnly,
    //     SendTickrate = 0
    // });

    public float flagPassRadius = 7;
    public Team team;

    private Vector3 spawnPos => (new Vector3(0,1.25f,120 * ((GetTeam() == Team.BLUE) ? 1 : -1)));

    Renderer rendererComponent;
    Renderer[] rendererComponents;
    // Start is called before the first frame update
    void Start()
    {
        rendererComponent = this.transform.GetChild(1).GetComponent<Renderer>();   
        rendererComponents = this.GetComponentsInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rendererComponent.material.color = (GetTeam() == Team.BLUE) ? new Color32(34,148,197,255) : new Color32(166,56,56,255);
        rendererComponent.material.SetColor("_EmissionColor", (GetTeam() == Team.BLUE) ? new Color32(34,148,197,255) : new Color32(191,7,5,255));

        if(capturer != null) {
            foreach(Renderer renderer in rendererComponents) {
                renderer.enabled = !capturer.isInvisToLocalPlayer;
            }
        }
        else {
            foreach(Renderer renderer in rendererComponents) {
                renderer.enabled = true;
            }
        }
    }

    public Team GetTeam() {
        return team;
    }

    public void ClientHandoverFlag() {
        HandoverFlagServerRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    void HandoverFlagServerRpc() {
        if(!IsServer) return;
        Collider[] hits = Physics.OverlapSphere(this.transform.position, flagPassRadius);
        foreach(Collider collider in hits) {
            Player player = collider.gameObject.GetComponent<Player>();
            if(player == null) continue;
            if(player.team == capturer.team && player.localPlayer != capturer) {                
                Debug.Log($"Passing from {capturer.gameObject.name}{capturer.OwnerClientId} to {player.gameObject.name}{player.OwnerClientId}");
                GameManager.Instance.FlagPassed(player, capturer.syncPlayer);
                capturer = player.localPlayer;
                CapturedClientRpc(player.OwnerClientId);
                return;
            }
        }
    }

    void OnTriggerEnter(Collider collider) {        
        if(!IsServer) { return; }
        if(!GameManager.Instance.roundInProgress) return;
        Player player = collider.gameObject.GetComponent<Player>();
        if(player == null) return;
        bool isDifferentTeam = (player.team != this.GetTeam());
        bool isAvailableForCapture = (capturer == null);
        if(isDifferentTeam && isAvailableForCapture) {
            capturer = player.localPlayer;
            GameManager.Instance.FlagCapturedBy(player);
            CapturedClientRpc(player.OwnerClientId);
        }
    }

    [ClientRpc]
    private void CapturedClientRpc(ulong byClientId) {
        if(!GameManager.Instance.roundInProgress) return;
        
        LocalPlayer player = LocalPlayer.WithClientId(byClientId);
        Debug.Log(player.gameObject.name + "has received the flag....");
        this.transform.SetParent(player.flagSlot.transform);
        this.transform.localRotation = Quaternion.identity;
        this.transform.localPosition = Vector3.zero;
        capturer = player;
    }
    
    public void ResetPosition() {
        this.capturer = null;
        this.transform.SetParent(null);
        this.transform.position = spawnPos;
        this.transform.rotation = Quaternion.identity;
    }

    public bool hasReset {
        get {
            return this.capturer == null && this.transform.parent == null && Vector3.Distance(spawnPos, this.transform.position) < 0.1f;
        }
    }
}
