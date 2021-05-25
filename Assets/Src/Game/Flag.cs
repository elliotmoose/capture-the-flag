using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
public class Flag : NetworkBehaviour
{
    Player capturer;
    int team = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(!IsServer) { return; }
    }

    void OnTriggerEnter(Collider collider) {
        if(!IsServer) { return; }

        Player player = collider.gameObject.GetComponent<Player>();
        if(player.team != team && capturer == null) {
            this.transform.SetParent(player.flagSlot.transform);
            this.transform.localRotation = Quaternion.identity;
            this.transform.localPosition = Vector3.zero;
            capturer = player;
        }
    }

    public void SetTeam(int team) {
        if(!IsServer) { return; }
        this.team = team;
        this.transform.GetChild(1).GetComponent<Renderer>().material.color = team == 0 ? new Color32(166,56,56,255) : new Color32(34,148,197,255);
    }

    public void ResetPosition() {
        if(!IsServer) { return; }
        this.capturer = null;
        this.transform.position = new Vector3(0,1.25f,120 * ((team == 0) ? -1 : 1));
        this.transform.rotation = Quaternion.identity;
    }
}
