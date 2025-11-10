using TMPro;
using UnityEngine;

public class CooldownSkillHud : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] TextMeshProUGUI cooldownText;
    private ICooldownSkill skill;

    private void LateUpdate()
    {
        cooldownText.text = string.Format("{0:F1}", skill.CooldownTimer.CurrentTime); 
    }

    public void LinkToSkill(ICooldownSkill skill)
    {
        #if UNITY_EDITOR
        if(this.skill != null)
        {
            Debug.LogError("CooldownSkillHud cannot be linked to more than one cooldown skill!");
        }
        #endif

        skill.CooldownTimer.Began += OnCooldownBegan;
        skill.CooldownTimer.Halted += OnCooldowHalted;
        this.skill = skill;
    }

    public void UnlinkFromSkill()
    {
        if(skill == null)
        {
            return;
        }

        skill.CooldownTimer.Began -= OnCooldownBegan;
        skill.CooldownTimer.Halted -= OnCooldowHalted;
        skill = null;
    }

    private void OnCooldownBegan()
    {
        gameObject.SetActive(true);
    }

    private void OnCooldowHalted()
    {   
        gameObject.SetActive(false);
    }
}
