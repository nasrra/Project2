using Entropek.UnityUtils;
using UnityEditor;
using UnityEngine;


[UnityEditor.CustomEditor(typeof(ArmCannonSkill), true)]
[CanEditMultipleObjects]
public class ArmCannonSkillEditor : RuntimeEditor<ArmCannonSkill>
{
    private bool drawAttackRadiusVisualiser = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawAttackRadiusVisualiserToggle((ArmCannonSkill)target);
    }

    protected override void OnSceneGUI()
    {
        base.OnSceneGUI();
        
        if (drawAttackRadiusVisualiser)
        {
            DrawAttackRadiusVisualiser((ArmCannonSkill)target);
        }
    }

    protected void DrawAttackRadiusVisualiserToggle(ArmCannonSkill armCannonSkill)
    {
        drawAttackRadiusVisualiser = EditorGUILayout.BeginToggleGroup("Attack Radius Visualiser", drawAttackRadiusVisualiser);
        EditorGUILayout.EndToggleGroup();
    }

    protected void DrawAttackRadiusVisualiser(ArmCannonSkill armCannonSkill)
    {
        Vector3 center = armCannonSkill.transform.position;
        float radius = armCannonSkill.AttackRadius;

        Handles.color = Color.green * 0.5f;

        // draw wireframe sphere.

        Handles.SphereHandleCap(
            0,
            center,
            Quaternion.identity,
            radius * 2, // *2 as sphere 'size' is actually calculatewith diameter (radius = diameter * 2)
            EventType.Repaint
        );
    }
}
