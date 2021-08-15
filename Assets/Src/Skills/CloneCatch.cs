using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneCatch : Catch
{

    public CloneCatch() : base()
    {
        name = "Clone Catch";
        this.icon = PrefabsManager.Instance.cloneCatchIcon;
        this.description = "Catching any other N.O.D allows the Rogue to clone its skill";
    }

    // public override void UseSkill(LocalPlayer player)
    // {
    //     base.UseSkill(player);
    //     Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, player.syncPlayer.GetCatchRadius());

    //     foreach (Collider c in hitColliders)
    //     {
    //         LocalPlayer target = c.gameObject.GetComponent<LocalPlayer>();

    //         if (target == null) continue;
    //         if(target == player) continue;
    //         if(target.skills.Count < 2) {
    //             Debug.Log("Target does not have enough skills");
    //             continue;
    //         }

    //         Debug.Log("Cloning...");
    //         target.TakeNetworkEffect(EffectType.Cloned, player.OwnerClientId);

    //         Skill skill = target.skills[1];
    //         if (player.skills.Count == 2)
    //         {
    //             player.skills[1] = skill;
    //         }
    //         else if (player.skills.Count == 1)
    //         {
    //             player.skills.Add(skill);
    //         }

    //         break;
    //     }
    // }

    bool cloneUsedForThisCast = false;
    public override void UseSkill(LocalPlayer caster)
    {
        this.cloneUsedForThisCast = false;
        base.UseSkill(caster);
    }

    protected override void OnContact(Player target)
    {
        base.OnContact(target);

        if(!cloneUsedForThisCast) {
            if(caster is Rogue) {
                ((Rogue) caster).TriggerSkillSteal(target.OwnerClientId);
                cloneUsedForThisCast = true;
            }
        }
    }

}
