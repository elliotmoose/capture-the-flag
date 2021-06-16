using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Adept : Player
{
    void Awake()
    {
        moveSpeed = 15;
        catchSkill = new Catch(10);
        skills.Add(new Teleport());
        skills.Add(new Slow());
        
        OnAnimationStart += (string animationName) => {
            if(animationName == "Teleport") SpawnTeleportStartParticle();
        };
        OnAnimationEnd += (string animationName) => {
            if(animationName == "Teleport") SpawnTeleportEndParticle();
        };
    }    

    void SpawnTeleportStartParticle() {
        GameObject.Instantiate(PrefabsManager.Instance.teleportField, this.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
    }
    
    void SpawnTeleportEndParticle() {
        GameObject.Instantiate(PrefabsManager.Instance.teleportField, this.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
    }
}
