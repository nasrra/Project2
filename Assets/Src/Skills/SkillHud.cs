using UnityEngine;

public class SkillHud : ScriptableObject
{
    [SerializeField] Texture2D icon;
    public Texture2D Icon => icon;

    [SerializeField] string description;
    public string Description => description;
}
