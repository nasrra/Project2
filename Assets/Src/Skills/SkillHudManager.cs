using System;
using Entropek.Exceptions;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class SkillHudManager : MonoBehaviour
{
    public static SkillHudManager Singleton {get; private set;}

    [Header("Components")]
    [SerializeField] private SkillHud[] skillHuds;


    /// 
    /// Base.
    /// 


    void Awake()
    {
        if(Singleton == null)
        {
            Singleton = this;
        }
        else if(Singleton != this)
        {
            throw new SingletonException(nameof(SkillHudManager));
        }
    }

    void OnDestroy()
    {
        if (Singleton == this)
        {
            Singleton = null;
        }
    }


    ///
    /// Functions.
    /// 


    /// <summary>
    /// Links this the stored skill huds to a respective skill.
    /// </summary>
    /// <param name="skills"></param>

    public void LinkToSkills(in Skill[] skills)
    {
        Debug.Log("link to skills");
        for(int i = 0; i < skills.Length; i++)
        {
            skillHuds[i].LinkToSkill(skills[i]);
        }
    }


    /// <summary>
    /// Unlinks all stored skill huds from their linked skills.
    /// </summary>

    public void UnlinkFromSkills()
    {
        for(int i = 0; i < skillHuds.Length; i++)
        {
            skillHuds[i].UnlinkFromSkill();
        }
    }
}
