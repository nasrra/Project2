using System;
using Entropek.UnityUtils.Attributes;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [Header(nameof(Skill)+" Data")]
    [SerializeField] SkillHudData skillHudData;
    public SkillHudData SkillHudData => skillHudData;

    [SerializeField] public Player Player;

    [RuntimeField] protected bool inUse = false;
    public bool InUse => inUse;

    protected virtual void Awake()
    {
        GetInterfaceTypes();
        LinkEvents();
    }

    protected virtual void OnDestroy()
    {
        UnlinkEvents();
    }

    /// <summary>
    /// Gets all implemented interfaces of this skill and chaches them.
    /// </summary>

    protected abstract void GetInterfaceTypes();

    /// <summary>
    /// Checks whether or not this skill can be used based on its implementing skill interface conditions.
    /// </summary>
    /// <returns>true, if this skill can be used; otherwise false.</returns>

    public abstract bool CanUse();

    /// <summary>
    /// Use this skill.
    /// </summary>
    /// <returns>true, if the skill was successfully used; otherwise false.</returns>

    public bool Use()
    {
        if (CanUse() == false)
        {
            return false;
        }
        UseInternal();
        return true;
    }

    /// <summary>
    /// The internal method implementation for subclass skills when Use is called on a skill. 
    /// </summary>
    /// <returns></returns>

    protected abstract void UseInternal();

    /// <summary>
    /// Links all the events of this skill and its implementing interfaces.
    /// </summary>

    protected abstract void LinkEvents();

    /// <summary>
    /// Unlinks all the events of this skill and its implementing interfaces.
    /// </summary>

    protected abstract void UnlinkEvents();

}
