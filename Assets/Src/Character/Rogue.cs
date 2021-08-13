using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Messaging;
public class Rogue : LocalPlayer
{    
    public static float EVADE_CHANCE = 0.2f;
    private void Awake()
    {
        baseMoveSpeed = 18;
        catchSkill = new CloneCatch();
        skills.Add(new Invisibility());
    }

    public void TriggerSkillSteal(ulong playerClientId) {
        if(!IsServer) return;
        StealSkillFromPlayerClientRpc(playerClientId);
    }

    //on all clients, update stolen skill, and show vfx
    [ClientRpc]
    void StealSkillFromPlayerClientRpc(ulong playerClientId) {
        LocalPlayer target = LocalPlayer.WithClientId(playerClientId);
        if(target == null) return;
    
        if(target.skills.Count < 2) {
            Debug.Log("Target does not have enough skills");
            return;
        }

        Skill skill = target.skills[1];
        if (this.skills.Count == 2)
        {
            this.skills[1] = skill;
        }
        else if (this.skills.Count == 1)
        {
            this.skills.Add(skill);
        }

        //clone vfx
        GameObject trail = GameObject.Instantiate(PrefabsManager.Instance.cloneTrail, this.transform.position, Quaternion.identity);
        trail.GetComponent<FollowPlayer>().fromPlayer = target.gameObject;
        trail.GetComponent<FollowPlayer>().toPlayer = this.gameObject;
    }
}
