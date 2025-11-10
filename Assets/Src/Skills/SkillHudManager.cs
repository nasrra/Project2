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

    public void LinkToSkills(in Skill[] skills)
    {
        for(int i = 0; i < skills.Length; i++)
        {
            skillHuds[i].LinkToSkill(skills[i]);
        }
    }
}
