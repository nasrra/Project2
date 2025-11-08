using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Entropek.UnityUtils.Attributes
{
    [CustomPropertyDrawer(typeof(NavMeshAgentTypeField))]
    public class NavMeshAgentFieldPropertyDrawer : PropertyDrawer
    {
        // Note:
        static string[] agentTypeNames;
        static int[] agentTypeIds;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUILayout.HelpBox($"The [{nameof(NavMeshAgentTypeField)}] Attribute can only be implemented by an Integer field.", MessageType.Error);
            }

            // Retrieve agent names if we havent done so already.

            if(agentTypeNames==null || agentTypeIds==null)
            {
                int count = NavMesh.GetSettingsCount();
        
                agentTypeNames = new string[count]; 
                agentTypeIds = new int[count];

                for(int i = 0; i < count; i++)
                {
                    NavMeshBuildSettings settings = NavMesh.GetSettingsByIndex(i);
                    
                    agentTypeNames[i] = NavMesh.GetSettingsNameFromID(settings.agentTypeID);
                    agentTypeIds[i] = settings.agentTypeID;
                }
            }

            // retrieve the index in the id list of the curent AgentId that is stored in the SerializedProperty.

            int currentIndex = 0;
            for(int i = 0; i < agentTypeIds.Length; i++)
            {
                if(agentTypeIds[i] == property.intValue)
                {
                    currentIndex = i;
                }
            }

            // stay within the bounds of the agent types.

            currentIndex = Mathf.Clamp(currentIndex, 0, agentTypeNames.Length);

            // draw popup.

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, agentTypeNames);
            
            // get the type id of this OnGUI tick. 

            int newTypeId = agentTypeIds[newIndex];

            // assign when changed type id has changed.

            if(property.intValue != newTypeId)
            {
                property.intValue = newTypeId;
                // Debug.Log(NavMesh.GetSettingsNameFromID(property.intValue) +" "+currentIndex);
            }
        }
    }    
}

