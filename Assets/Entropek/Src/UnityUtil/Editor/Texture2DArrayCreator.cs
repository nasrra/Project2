using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Entropek.UnityUtils
{
    public class Texture2DArrayCreator : EditorWindow
    {

        private ReorderableList texturesList;
        private List<Texture2D> textures = new();
        private string saveDirectory = "TextureArray's";
        private string fileName = "NewTextureArray";
        private bool generateMipMaps = false;

        [MenuItem("Entropek/Rendering/Texture2DArray Creator")]
        public static void ShowWindow()
        {
            // Opens the window or focuses it if already open.
            GetWindow<Texture2DArrayCreator>();
        }

        private void OnEnable()
        {
            // create the ReorderableList.
            
            texturesList = new ReorderableList(
                textures,
                typeof(Texture2D),
                true,   // draggable
                true,   // display header
                true,   // show add button
                true    // show remove button.
            );

            // header

            texturesList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Textures (Drag & Drop Here)");  
            };
        
            // Element drawer

            texturesList.drawElementCallback = (Rect rect, int index, bool active, bool focused) =>
            {
                rect.y += 2;

                textures[index] = (Texture2D)EditorGUI.ObjectField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    textures[index],
                    typeof(Texture2D),
                    false
                );
            };

            // accept drag-and-drop directly onto the list area.

            texturesList.onAddCallback = (ReorderableList list) =>
            {
                textures.Add(null);
            };

            texturesList.onCanRemoveCallback = list => list.count > 0;
        }

        void OnGUI()
        {
            GUILayout.Label("Texture 2D Creator", EditorStyles.boldLabel);
            
            GUILayout.Space(5);
        
            saveDirectory = EditorGUILayout.TextField("Save Directory", saveDirectory);
            fileName = EditorGUILayout.TextField("File Name", fileName);
            generateMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", generateMipMaps);

            texturesList.DoLayoutList();


            if (GUILayout.Button("Generate"))
            {
                GenerateTextureArray();
            }
        }

        private void GenerateTextureArray()
        {
            if(textures.Count == 0)
            {
                Debug.LogError("Unable to generate texture array: No textures assigned for texture array creation."); 
                return;
            }

            // set defaults so the compiler isnt angry.

            int width = 0;
            int height = 0;
            TextureFormat format = TextureFormat.RGBA32;

            for(int i = 0; i < textures.Count; i++)
            {
                Texture2D texture = textures[i];

                if (texture == null)
                {
                    Debug.LogError("Unable to generate texture array: One or more entries in the texture list are null.");
                    return;
                }

                if (i == 0)
                {                    
                    width = texture.width;
                    height = texture.height;
                    format = texture.format;
                }

                if (texture.width != width || texture.height != height)
                {
                    Debug.LogError("Unable to generate texture array: All textures must be the same resolution.");
                    return;
                }

                if(texture.format != format)
                {
                    Debug.LogError($"Unable to generate texture array: All textures must be the same format ({format} not {texture.format})");
                    return;
                }

                
            }
            
            // create the array.
            
            Texture2DArray texture2DArray = new Texture2DArray(width, height, textures.Count, format, generateMipMaps);

            // copy each texture into the array.

            for(int i = 0; i < textures.Count; i++)
            {
                Graphics.CopyTexture(textures[i], 0, 0, texture2DArray, i, 0);
            }

            // apply to upload to the GPU and actually generate the 
            // texture 2D array for shader usage.

            texture2DArray.Apply();

            // Save as an asset.

            string assetDirectory = $"Assets/{saveDirectory}"; 
            string assetPath = $"{assetDirectory}{fileName}.asset";

            if(Directory.Exists(assetDirectory) == false)
            {
                Directory.CreateDirectory(assetDirectory);
                Debug.Log($"Created director {assetDirectory}");
            }

            AssetDatabase.CreateAsset(texture2DArray, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.SaveAssets();

            Debug.Log("Successfully created Texture2DArray");
        }
    }    
}

