using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Создание ассетов и объектов: ArtMeshAsset с видом Base, материал по умолчанию, GameObject с ArtMeshRenderer.</summary>
    public static class ArtMeshAssetCreator
    {
        const string MaterialFolder = "Assets/ArtMesh";
        const string MaterialPath = MaterialFolder + "/ArtMeshDefault.mat";

        public static ArtMeshAsset CreateAndAssign(ArtMeshRenderer r)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create ArtMesh Asset", r.name + "_ArtMesh", "asset", "");
            if (string.IsNullOrEmpty(path)) return null;

            var a = ScriptableObject.CreateInstance<ArtMeshAsset>();
            AssetDatabase.CreateAsset(a, path);
            ViewOperations.AddBase(a);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(r, "Assign ArtMesh asset");
            r.Asset = a;
            EditorUtility.SetDirty(r);
            return a;
        }

        /// <summary>Материал на шейдере ArtMesh/FlatDeform; создаётся один раз в Assets/ArtMesh.</summary>
        public static Material EnsureDefaultMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat != null) return mat;

            var shader = Shader.Find("ArtMesh/FlatDeform");
            if (shader == null) { Debug.LogError("ArtMesh: шейдер ArtMesh/FlatDeform не найден."); return null; }
            if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets", "ArtMesh");
            mat = new Material(shader) { name = "ArtMeshDefault" };
            AssetDatabase.CreateAsset(mat, MaterialPath);
            AssetDatabase.SaveAssets();
            return mat;
        }

        [MenuItem("GameObject/ArtMesh/Art Mesh Part", false, 10)]
        static void CreatePart(MenuCommand cmd)
        {
            var go = new GameObject("ArtMesh Part");
            GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create ArtMesh Part");

            var r = go.AddComponent<ArtMeshRenderer>();
            var mat = EnsureDefaultMaterial();
            if (mat != null)
            {
                using (var so = new SerializedObject(r))
                {
                    so.FindProperty("material").objectReferenceValue = mat;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            Selection.activeGameObject = go;
            ArtMeshEditorWindow.OpenFor(r);
        }
    }
}
