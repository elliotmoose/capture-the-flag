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
    public List<Effect> effects = new List<Effect>();
    protected float cdTimer1 = 0.0f;
    protected float cdTimer2 = 0.0f;

    
    
    

    public NetworkVariableULong ownerClientId = new NetworkVariableULong(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

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

        UpdateEffects(); // update skill effects applied to player
        if (this.ToString()=="TestSubject (TestSubject)")
        {
            Debug.Log(this.moveSpeed);
        }       

    }

    void FixedUpdate()
    {
        // use first skill if F is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Time.time > cdTimer1) {
                Skill skill = skills[0];
                skill.UseSkill(this);
                cdTimer1 = Time.time + skill.cooldown;
            }
            else
            {
                float timeLeft = cdTimer1 - Time.time;
                Debug.Log("Cooldown time left: " + timeLeft.ToString() + " seconds");
            }
        }

        // use second skill if E is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Time.time > cdTimer2)
            {
                Skill skill = skills[1];
                skill.UseSkill(this);
                cdTimer2 = Time.time + skill.cooldown;
                
            }
            else
            {
                float timeLeft = cdTimer2 - Time.time;
                Debug.Log("Cooldown time left: " + timeLeft.ToString() + " seconds");
            }
        }

    }

    void LateUpdate() {
        if(NetworkManager.LocalClientId == ownerClientId.Value) {
            transform.rotation = Quaternion.Euler(transform.rotation.x, Camera.main.transform.eulerAngles.y, transform.rotation.z);
        }
    }


    // receive effect
    public void TakeEffect(Effect effect)
    {
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
        
    }

    public void UpdateEffects()
    {
        //Debug.Log(this.ToString()+this.effects.Count.ToString());
        foreach (Effect effect in this.effects)
        {
            effect.Update();
            if (effect.effectEnded)
            {
                effects.Remove(effect);
            }
        }
    }

}
