using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillHud : MonoBehaviour
{

    
    [Header("Components")]
    [SerializeField] RawImage icon;
    
    [SerializeField] GameObject BatchChargesHud;
    [SerializeField] TextMeshProUGUI batchChargesText;

    [SerializeField] CooldownSkillHud cooldownHud;

    private SkillHudData hudData;
    private Skill skill;


    /// 
    /// Base.
    /// 


    private void OnDestroy()
    {
        UnlinkFromSkill();
    }


    ///
    /// Functions.
    /// 


    /// <summary>
    /// Links a skill to this instance.
    /// </summary>
    /// <param name="skill">The skill to link to.</param>
    /// <exception cref="System.Exception">Thrown when this instance is already linked to a Skill.</exception>

    public void LinkToSkill(in Skill skill)
    {
        if(this.skill != null)
        {
            throw new System.Exception("SkillHud can only be linked to one Skill at a time!");
        }

        hudData = skill.SkillHudData;
        icon.texture = hudData.Icon;

        if(skill is ICooldownSkill cooldownSkill)
        {
            cooldownHud.LinkToSkill(cooldownSkill);
        }

        this.skill = skill;
    }

    /// <summary>
    /// Unlinks this instance from the currently linked Skill.
    /// </summary>

    public void UnlinkFromSkill()
    {
        if(skill == null)
        {
            return;
        }

        if(skill is ICooldownSkill)
        {
            cooldownHud.UnlinkFromSkill();
        }

        skill = null;
    }
}
