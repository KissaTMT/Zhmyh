using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>Минимальный инспектор компонента: стандартные поля и кнопка открытия окна. Вся работа в ArtDetails Editor.</summary>
    [CustomEditor(typeof(ArtDetail))]
    public sealed class ArtDetailInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var d = (ArtDetail)target;
            EditorGUILayout.Space();
            foreach (var issue in DetailValidator.Validate(d))
                EditorGUILayout.HelpBox(issue.text, issue.isError ? MessageType.Error : MessageType.Warning);
            if (GUILayout.Button("Открыть ArtDetails Editor")) ArtDetailEditorWindow.OpenFor(d);
        }
    }
}
