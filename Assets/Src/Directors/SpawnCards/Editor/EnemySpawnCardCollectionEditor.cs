using Entropek.UnityUtils;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(EnemySpawnCardCollection))]
public class EnemySpawnCardCollectionEditor : RuntimeEditor<EnemySpawnCardCollection>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Set Min and Max Cost's"))
        {
            EnemySpawnCardCollection enemySpawnCardCollection = target as EnemySpawnCardCollection;
            enemySpawnCardCollection.SetMinAndMaxCardCost();
            RepaintEditor(enemySpawnCardCollection);
        }

        if(GUILayout.Button("Sort by Cost Descending."))
        {
            EnemySpawnCardCollection enemySpawnCardCollection = target as EnemySpawnCardCollection;
            enemySpawnCardCollection.SortByCostDescending();
            RepaintEditor(enemySpawnCardCollection);
        }
    }

    private void RepaintEditor(EnemySpawnCardCollection enemySpawnCardCollection)
    {
        // Tell Unity that the ScriptableObject has changed.
        
        UnityEditor.EditorUtility.SetDirty(enemySpawnCardCollection);

        // Refresh the serialized object to show updated values.

        serializedObject.Update();
        
        // repaint the editor to display the new values.

        Repaint();
    }
}
