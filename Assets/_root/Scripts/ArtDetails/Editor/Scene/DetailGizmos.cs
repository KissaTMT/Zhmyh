using ArtMesh;
using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>
    /// Гизмо детали: точка привязки (красная сфера), привязанный треугольник (оранжевый) и силуэт-маска (зелёный).
    /// Рисуется у выбранной детали или у всех, если окно ArtDetails включило AlwaysVisible.
    /// </summary>
    public static class DetailGizmos
    {
        public static bool AlwaysVisible;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void Draw(ArtDetail d, GizmoType type)
        {
            if (!AlwaysVisible && (type & GizmoType.Selected) == 0) return;
            var target = d.Target;
            if (target == null) return;

            Gizmos.matrix = target.transform.localToWorldMatrix;
            DrawSilhouette(target);
            DrawBinding(d, target);
            Gizmos.matrix = Matrix4x4.identity;
        }

        static void DrawSilhouette(ArtMeshRenderer target)
        {
            int n = SilhouetteCache.Get(target, out Vector2[] pts);
            if (n < 3) return;
            Gizmos.color = new Color(.2f, 1f, .4f, .9f);
            for (int i = 0; i < n; i++)
            {
                Vector2 a = pts[i], b = pts[(i + 1) % n];
                Gizmos.DrawLine(new Vector3(a.x, a.y, 0f), new Vector3(b.x, b.y, 0f));
            }
        }

        static void DrawBinding(ArtDetail d, ArtMeshRenderer target)
        {
            if (!d.HasAnchor) return;
            Gizmos.color = new Color(1f, .15f, .15f);
            float size = 0.02f;
            Gizmos.DrawSphere(d.Anchor, size);

            var asset = target.Asset;
            var g = asset != null ? asset.Geometry : null;
            var b = d.Binding;
            if (g == null || !g.IsValid || b.triangle * 3 + 2 >= g.triangles.Length) return;

            var st = target.State;
            Vector3 p0 = ArtMeshEvaluator.FinalPos(asset, g, st, g.triangles[b.triangle * 3]);
            Vector3 p1 = ArtMeshEvaluator.FinalPos(asset, g, st, g.triangles[b.triangle * 3 + 1]);
            Vector3 p2 = ArtMeshEvaluator.FinalPos(asset, g, st, g.triangles[b.triangle * 3 + 2]);
            Gizmos.color = new Color(1f, .55f, .1f);
            Gizmos.DrawLine(p0, p1); Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p0);
        }
    }
}
