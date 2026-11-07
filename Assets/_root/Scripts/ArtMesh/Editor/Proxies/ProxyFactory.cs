using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Создаёт и удаляет прокси-объекты. Обычные GameObject без собственных компонентов:
    /// не сохраняются в сцену и не попадают в билд (HideFlags.DontSaveInEditor | DontSaveInBuild).
    /// </summary>
    public static class ProxyFactory
    {
        public const string RootName = "ArtMesh Proxies";
        const HideFlags Flags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

        public static ProxySession Create(ArtMeshRenderer r, int viewIndex, ProxyKind kind, ProxyOptions options, out string error)
        {
            error = null;
            var a = r != null ? r.Asset : null;
            if (a == null || a.mesh == null) { error = "У рендерера нет ассета с запечённым мешем (вкладка Mesh → Bake)."; return null; }
            if (viewIndex < 0 || viewIndex >= a.views.Count) { error = "Не выбран вид."; return null; }

            CleanupOrphans(r);
            a.EnsureViewData();
            var view = a.views[viewIndex];

            int n;
            if (kind == ProxyKind.Lattice) n = a.LatticeCount;
            else
            {
                var g = a.Geometry;
                if (g == null || !g.IsValid) { error = "Меш не содержит данных ArtMesh. Сделай Bake заново."; return null; }
                n = a.VertexCount;
                if (view.vertexDelta == null || view.vertexDelta.Length != n)
                {
                    Undo.RecordObject(a, "Create vertex edits");
                    view.vertexDelta = new Vector3[n];
                    EditorUtility.SetDirty(a);
                }
            }

            r.Refresh();

            var s = new ProxySession
            {
                renderer = r, viewIndex = viewIndex, kind = kind, options = options,
                cols = a.latticeCols, rows = a.latticeRows
            };
            s.root = new GameObject(RootName) { hideFlags = Flags };
            s.root.transform.SetParent(r.transform, false);
            ProxyGeometry.PlaceRoot(s);

            s.points = new Transform[n];
            for (int i = 0; i < n; i++)
            {
                string name = kind == ProxyKind.Lattice ? $"n {i % s.cols},{i / s.cols}" : $"v {i}";
                var go = new GameObject(name) { hideFlags = Flags };
                go.transform.SetParent(s.root.transform, false);
                s.points[i] = go.transform;
                ProxyGeometry.Place(s, i);
            }
            ProxyRegistry.Register(s);
            return s;
        }

        public static void Destroy(ProxySession s)
        {
            if (s == null) return;
            ProxyRegistry.Unregister(s);
            if (s.root != null) UnityEngine.Object.DestroyImmediate(s.root);
            s.root = null;
        }

        /// <summary>Удаляет прокси, оставшиеся после перезагрузки скриптов.</summary>
        public static void CleanupOrphans(ArtMeshRenderer r)
        {
            if (r == null) return;
            for (int i = r.transform.childCount - 1; i >= 0; i--)
            {
                var c = r.transform.GetChild(i);
                if (c.name == RootName) UnityEngine.Object.DestroyImmediate(c.gameObject);
            }
        }
    }
}
