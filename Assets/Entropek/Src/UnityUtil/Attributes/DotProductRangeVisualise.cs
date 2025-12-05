using UnityEditor;
using UnityEngine;

namespace Entropek.UnityUtils.Attributes
{
    /// <summary>
    /// This is a custom attribute for serialised field,
    /// displaying an 
    /// </summary>

    public class DotProductRangeVisualise : AdvancedPropertyAttribute
    {
        private bool drawVisualiser = false;
        [SerializeField] private float minAngle;
        [SerializeField] private float maxAngle;

        private static Color OutsideFovColor = Color.red;
        private static Color InsideFovColor = Color.green;

        private const string MaxFieldName = "max";
        private const string MinFieldName = "min";

        #if UNITY_EDITOR

        public override void OnInspectorGUI(SerializedProperty serializedProperty)
        {
            EditorGUILayout.PropertyField(serializedProperty, true);
            DrawToggle();

            // get the stored values.

            var min = serializedProperty.FindPropertyRelative(MinFieldName);
            var max = serializedProperty.FindPropertyRelative(MaxFieldName);

            // regulate them so min is never greater then max; and vice versa.

            DotProductRange regulated = DotProductRange.RegulateValues(min.floatValue, max.floatValue);
            min.floatValue = regulated.Min;
            max.floatValue = regulated.Max;

            // calculate the angles.

            minAngle = regulated.GetMinAngle();
            maxAngle = regulated.GetMaxAngle();       
        }

        public override void OnSceneGUI(Object target)
        {
            if (drawVisualiser == true)
            {
                DrawVisualiser(target, maxAngle, minAngle);
            }     
        }


        private void DrawToggle()
        {
            drawVisualiser = EditorGUILayout.BeginToggleGroup("Draw Dot Product Range Visualiser", drawVisualiser);
            EditorGUILayout.EndToggleGroup();
        }

        public static void DrawVisualiser(Object target, float maxAngle, float minAngle)
        {

            UnityEngine.Transform transform = ((MonoBehaviour)target).transform;


            // draw the user defined max angle, where the combat angent cannot see in relation to the front of it.

            DrawHorizontalFovArc(transform, OutsideFovColor, -maxAngle, maxAngle);
            DrawVerticalFovArc(transform, OutsideFovColor, 0, maxAngle);
            DrawVerticalFovArc(transform, OutsideFovColor, 0, -maxAngle);

            // draw the remaining area that is less than the minimum viewing angle,

            DrawHorizontalFovArc(transform, OutsideFovColor, -minAngle, -180);
            DrawHorizontalFovArc(transform, OutsideFovColor, minAngle, 180);
            DrawVerticalFovArc(transform, OutsideFovColor, -minAngle, -180);
            DrawVerticalFovArc(transform, OutsideFovColor, minAngle, 180);

            // draw the min angle, where the combat agent can consider the action when the target is within said angle.

            DrawHorizontalFovArc(transform, InsideFovColor, maxAngle, minAngle);
            DrawHorizontalFovArc(transform, InsideFovColor, -maxAngle, -minAngle);
            DrawVerticalFovArc(transform, InsideFovColor, -maxAngle, -minAngle);
            DrawVerticalFovArc(transform, InsideFovColor, maxAngle, minAngle);
        }

        /// <summary>
        /// Draws a filled arc along the local x-axis in relation to a transforms position. 
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="color">The fill color of the arc.</param>
        /// <param name="minAngle">The minimum angle of the arc.</param>
        /// <param name="maxAngle">The maximum angle of the ar.</param>

        private static void DrawHorizontalFovArc(UnityEngine.Transform transform, Color color, float minAngle, float maxAngle)
        {
            Handles.color = color;

            // Start direction (rotated from forward by minAngle around local up)
            Vector3 minDir = Quaternion.AngleAxis(minAngle, transform.up) * transform.forward;

            // Draw around local up axis
            Handles.DrawSolidArc(transform.position, transform.up, minDir, maxAngle - minAngle, 1);
        }

        /// <summary>
        /// Draws a filled arc along the local y-axis in relation to a transforms position. 
        /// </summary>
        /// <param name="transform">The specified transform.</param>
        /// <param name="color">The fill color of the arc.</param>
        /// <param name="minAngle">The minimum angle of the arc.</param>
        /// <param name="maxAngle">The maximum angle of the ar.</param>

        private static void DrawVerticalFovArc(UnityEngine.Transform transform, Color color, float minAngle, float maxAngle)
        {
            Handles.color = color;

            // Start direction (rotated up/down from forward around local right)
            Vector3 minDir = Quaternion.AngleAxis(minAngle, transform.right) * transform.forward;

            // Draw around local right axis
            Handles.DrawSolidArc(transform.position, transform.right, minDir, maxAngle - minAngle, 1);
        }
        #endif
    }
}
