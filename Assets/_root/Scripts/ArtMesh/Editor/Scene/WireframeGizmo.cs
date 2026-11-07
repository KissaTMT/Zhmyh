using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Каркас итоговой формы (CPU-вычисление того же, что делает шейдер). Рисуется у выбранной детали или всегда, если включён Enabled.</summary>
    public static class WireframeGizmo
    {
        /// <summary>Окно включает его на время редактирования вида (чтобы каркас был виден, когда выбраны прокси).</summary>
        public static bool Enabled;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void Draw(ArtMeshRenderer r, GizmoType type)
        {
            if (!Enabled && (type & GizmoType.Selected) == 0) return;
            var a = r.Asset;
            if (a == null || a.mesh == null) return;
            var g = a.Geometry;
            if (g == null || !g.IsValid) return;
            var st = r.State;
            if (st.lattice.Length != a.LatticeCount || st.vertexDelta.Length != g.VertexCount) return;

            var pos = new Vector3[g.VertexCount];
            for (int i = 0; i < pos.Length; i++) pos[i] = ArtMeshEvaluator.FinalPos(a, g, st, i);

            Gizmos.matrix = r.transform.localToWorldMatrix;
            Gizmos.color = new Color(.1f, .1f, .1f, .85f);
            var t = g.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 p0 = pos[t[i]], p1 = pos[t[i + 1]], p2 = pos[t[i + 2]];
                Gizmos.DrawLine(p0, p1); Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p0);
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
