using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Находит и создаёт ArtMeshAuthoring для ассета: файл «Имя.authoring.asset» в той же папке.</summary>
    public static class ArtMeshAuthoringRepository
    {
        public static string PathFor(ArtMeshAsset asset)
        {
            string p = asset != null ? AssetDatabase.GetAssetPath(asset) : null;
            return string.IsNullOrEmpty(p) ? null : Path.ChangeExtension(p, ".authoring.asset");
        }

        public static ArtMeshAuthoring Find(ArtMeshAsset asset)
        {
            string path = PathFor(asset);
            if (path == null) return null;
            var au = AssetDatabase.LoadAssetAtPath<ArtMeshAuthoring>(path);
            if (au != null && au.target != asset)
            {
                au.target = asset;
                EditorUtility.SetDirty(au);
            }
            return au;
        }

        public static ArtMeshAuthoring GetOrCreate(ArtMeshAsset asset)
        {
            var au = Find(asset);
            if (au != null) return au;

            string path = PathFor(asset);
            if (path == null) return null;
            au = ScriptableObject.CreateInstance<ArtMeshAuthoring>();
            au.target = asset;
            AssetDatabase.CreateAsset(au, path);
            AssetDatabase.SaveAssets();
            return au;
        }
    }
}
