using System;
using UnityEngine;

[Serializable]
public class SkillCollection
{
    [Header("Components")]
    [SerializeField] private Skill[] skills;
    public Skill[] Skills => skills;

    /// <summary>
    /// Uses a Skill stored by this SkillCollection.
    /// </summary>
    /// <param name="skillIndex">The index of the skill that is stored in the internal array.</param>
    /// <returns>true, if the skill was succssfully used; otherwise false.</returns>

    public bool UseSkill(int skillIndex)
    {
        return skills[skillIndex].Use();
    }

    /// <summary>
    /// Checks whether or not any animated skill stored by this SkillCollection is currently in use.
    /// </summary>
    /// <returns>true, if an IAnimatedSkill is in use; otherwise false.</returns>

    public bool AnimatedSkillIsInUse()
    {
        for(int i = 0; i < skills.Length; i++)
        {
            Skill skill = skills[i];
            if(skill.InUse == true && skill is IAnimatedSkill)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks Whether or not a skill in this SkillCollection is currently in use.
    /// </summary>
    /// <param name="skillIndex">The index of the skill that is stored in the internal array.</param>
    /// <returns>true, if the skill is currently in use; otherwise false.</returns>

    public bool SkillIsInUse(int skillIndex)
    {
        return skills[skillIndex].InUse;
    }
}
