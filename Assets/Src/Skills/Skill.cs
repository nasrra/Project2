using System;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [Header(nameof(Skill)+" Data")]
    [SerializeField] SkillHud skillHud;
    public SkillHud SkillHud => skillHud;

    [SerializeField] public Player Player;

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
    /// Use this skill.
    /// </summary>
    /// <returns>true, if the skill was successfully used; otherwise false.</returns>

    public abstract bool Use();

    /// <summary>
    /// Links all the events of this skill and its implementing interfaces.
    /// </summary>

    protected abstract void LinkEvents();

    /// <summary>
    /// Unlinks all the events of this skill and its implementing interfaces.
    /// </summary>

    protected abstract void UnlinkEvents();

    /// <summary>
    /// Gets all implemented interfaces of this skill and chaches them.
    /// </summary>

    protected abstract void GetInterfaceTypes();
}
