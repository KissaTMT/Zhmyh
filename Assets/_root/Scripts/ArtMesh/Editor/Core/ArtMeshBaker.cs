using System;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Запекает меш из Source Sprite (данные авторинга) в под-ассет ArtMeshAsset и обновляет зависящие данные:
    /// раскладку вершин, область сетки, данные видов.
    /// </summary>
    public static class ArtMeshBaker
    {
        public static bool Bake(ArtMeshAsset asset, ArtMeshAuthoring au, out string error)
        {
            error = null;
            if (asset == null) { error = "Нет ArtMeshAsset."; return false; }
            if (au == null) { error = "Нет данных авторинга (ассет должен лежать в проекте)."; return false; }
            if (au.sourceSprite == null) { error = "Не задан Source Sprite."; return false; }

            RadialMeshData d;
            Vector2 anchor = au.anchor;
            try { d = RadialMeshBuilder.Build(au.sourceSprite, au.build, ref anchor, au.autoAnchor); }
            catch (Exception e) { error = e.Message; return false; }

            Undo.RecordObject(asset, "Bake ArtMesh");
            Undo.RecordObject(au, "Bake ArtMesh");
            au.anchor = anchor;

            Mesh m = asset.mesh;
            bool created = m == null || !AssetDatabase.IsSubAsset(m);
            if (created) m = new Mesh { name = asset.name + "_Mesh" };
            else m.Clear();

            m.indexFormat = d.vertices.Length > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            m.vertices = d.vertices;
            m.SetUVs(0, d.normals);           // UV0 = нормаль обводки
            m.subMeshCount = 2;
            m.SetTriangles(d.triangles, 0);   // заливка
            m.SetTriangles(d.triangles, 1);   // обводка (те же треугольники)
            m.bounds = d.bounds;

            if (created) AssetDatabase.AddObjectToAsset(m, asset);
            asset.mesh = m;
            asset.topology.rays = au.build.rays;
            asset.topology.rings = au.build.rings;
            asset.topology.ringBias = au.build.ringBias;

            UpdateLatticeArea(asset, d.vertices);
            asset.InvalidateGeometry();
            asset.EnsureViewData();

            EditorUtility.SetDirty(m);
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(au);
            AssetDatabase.SaveAssets();
            return true;
        }

        /// <summary>Сетка симметрична относительно x = 0 (нужно для зеркальных видов) и чуть больше формы (под обводку).</summary>
        static void UpdateLatticeArea(ArtMeshAsset asset, Vector3[] vertices)
        {
            float hw = 0f, minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in vertices)
            {
                hw = Mathf.Max(hw, Mathf.Abs(v.x));
                minY = Mathf.Min(minY, v.y);
                maxY = Mathf.Max(maxY, v.y);
            }
            float pad = Mathf.Max(0.05f, (maxY - minY) * 0.15f);
            asset.latticeMin = new Vector2(-hw - pad, minY - pad);
            asset.latticeSize = new Vector2(2f * (hw + pad), maxY - minY + 2f * pad);
        }
    }
}
