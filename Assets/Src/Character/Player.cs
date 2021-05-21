using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    // Start is called before the first frame update    
    public Vector3 moveDir = Vector3.zero;

    // player stats and skills
    protected float moveSpeed = 10;
    protected List<Skill> skills = new List<Skill>();
    protected float cdTimer1 = Time.time;
    protected float cdTimer2 = Time.time;

    public NetworkVariableULong ownerClientId = new NetworkVariableULong(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    void Start()
    {
       
    }

    // Player() : base() {
    //     Debug.Log("player constructed");
    //     ownerClientId.OnValueChanged += (ulong oldId, ulong newId) => {
    //         this.name = "player"+newId.ToString();
    //         Debug.Log("onvaluechanged");

    //         //set camera
    //         if(newId == NetworkManager.Singleton.LocalClientId) {
    //             Camera.main.transform.SetParent(this.transform);
    //         }
    //     };

    //     Debug.Log("event subscribed");
    // }

    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            // Debug.Log(NetworkManager.Singleton.LocalClientId);
            transform.position += moveDir * Time.deltaTime * moveSpeed;
        }

    }

    void FixedUpdate()
    {
        // use first skill if F is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Time.time > cdTimer1) {
                Skill skill = skills[0];
                skill.useSkill(this);
                cdTimer1 = Time.time + skill.cooldown;
            }
            else
            {
                float timeLeft = cdTimer1 - Time.time;
                Debug.Log("Cooldown time left: " + timeLeft.ToString() + " seconds");
            }
            

        }

    }

    void LateUpdate() {
        if(NetworkManager.LocalClientId == ownerClientId.Value) {
            transform.rotation = Quaternion.Euler(transform.rotation.x, Camera.main.transform.eulerAngles.y, transform.rotation.z);
        }
    }
}
