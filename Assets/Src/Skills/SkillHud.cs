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
    private Action LateUpdateCallback;

    private void LateUpdate()
    {
        LateUpdateCallback?.Invoke();
    }

    public void LinkToSkill(in Skill skill)
    {
        hudData = skill.SkillHudData;
        icon.texture = hudData.Icon;

        if(skill is ICooldownSkill cooldownSkill)
        {
            cooldownHud.LinkToSkill(cooldownSkill);
        }
    }
}
