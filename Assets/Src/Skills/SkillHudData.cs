using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Skills/SkillHudData")]
public class SkillHudData : ScriptableObject
{
    [SerializeField] Texture2D icon;
    public Texture2D Icon => icon;

    [SerializeField] string description;
    public string Description => description;
}
