using UnityEngine;
using UnityEditor;

namespace MADCUP.STM
{
    [CustomEditor(typeof(SpriteMesh))]
    public class SpriteMeshEditor : Editor
    {
        SerializedProperty spriteProperty;
        SerializedProperty colorProperty;

        void OnEnable()
        {
            spriteProperty = serializedObject.FindProperty("sprite");
            colorProperty = serializedObject.FindProperty("color");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 120f;

            if (targets.Length > 1)
            {
                EditorGUILayout.HelpBox("Multi-object editing is supported. Properties may not be editable.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawPropertiesExcluding(serializedObject, "m_Script", "sprite", "color");

            EditorGUILayout.Space();

            Rect spriteRect = EditorGUILayout.GetControlRect(GUILayout.Height(30f), GUILayout.ExpandWidth(true));
            EditorGUI.PropertyField(spriteRect, spriteProperty, new GUIContent("Sprite"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(colorProperty, new GUIContent("Color"));

            EditorGUILayout.Space();

            SpriteMesh SpriteMesh = (SpriteMesh)target;
            SpriteRenderer spriteRenderer = SpriteMesh.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                EditorGUILayout.HelpBox("SpriteRenderer component detected. Consider converting to mesh.", MessageType.Warning);

                EditorGUILayout.Space();

                if (GUILayout.Button("Convert to Mesh", GUILayout.Height(30f)))
                {
                    SpriteMesh.sprite = spriteRenderer.sprite;

                    Undo.RecordObject(SpriteMesh.gameObject, "Convert to Mesh");
                    DestroyImmediate(spriteRenderer, true);

                    MeshFilter meshFilter = SpriteMesh.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                    {
                        meshFilter = SpriteMesh.gameObject.AddComponent<MeshFilter>();
                    }

                    MeshRenderer meshRenderer = SpriteMesh.GetComponent<MeshRenderer>();
                    if (meshRenderer == null)
                    {
                        meshRenderer = SpriteMesh.gameObject.AddComponent<MeshRenderer>();
                    }

                    SpriteMesh.Initialize();

                    Debug.Log("Converted Done!!");
                }
            }
            else
            {
                MeshRenderer meshRenderer = SpriteMesh.GetComponent<MeshRenderer>();
                if (meshRenderer == null)
                {
                    MeshFilter meshFilter = SpriteMesh.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                    {
                        meshFilter = SpriteMesh.gameObject.AddComponent<MeshFilter>();
                    }
                    meshRenderer = SpriteMesh.gameObject.AddComponent<MeshRenderer>();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override void OnHeaderGUI()
        {
            if (targets.Length > 1)
            {
                GUILayout.Label("SpriteMesh (Multi-edit)", EditorStyles.boldLabel);
            }
            else
            {
                base.OnHeaderGUI();
            }
        }
    }
}
