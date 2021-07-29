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

    public Team team;

    private Vector3 spawnPos => (new Vector3(0,1.25f,120 * ((GetTeam() == Team.BLUE) ? 1 : -1)));

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

        if(capturer != null) {
            foreach(Renderer renderer in this.transform.GetComponentsInChildren<Renderer>()) {
                renderer.enabled = !capturer.isInvisToLocalPlayer;
            }
        }
    }

    public Team GetTeam() {
        return team;
    }

    void OnTriggerEnter(Collider collider) {        
        if(!IsServer) { return; }
        if(!GameManager.Instance.roundInProgress) return;
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
        Debug.Log("Flag captured received on client!");
        if(!GameManager.Instance.roundInProgress) return;
        LocalPlayer player = LocalPlayer.WithClientId(byClientId);
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
