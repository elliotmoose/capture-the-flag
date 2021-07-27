using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDescription : Catch
{
    public SkillDescription(string name, Sprite icon, string description) : base()
    {
        this.name = name;
        this.icon = icon;
        this.description = description;
        this.cooldown = 0;
    }

    public override void UseSkill(LocalPlayer player)
    {
        
    }

}
