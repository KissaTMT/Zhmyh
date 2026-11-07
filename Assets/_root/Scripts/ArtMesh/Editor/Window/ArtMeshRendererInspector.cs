using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Минимальный инспектор компонента: стандартные поля и кнопка открытия окна. Вся работа в ArtMesh Editor.</summary>
    [CustomEditor(typeof(ArtMeshRenderer))]
    public sealed class ArtMeshRendererInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var r = (ArtMeshRenderer)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Yaw {r.Yaw:0.#}°   Pitch {r.Pitch:0.#}°", EditorStyles.miniLabel);
            if (GUILayout.Button("Открыть ArtMesh Editor")) ArtMeshEditorWindow.OpenFor(r);
        }
    }
}
