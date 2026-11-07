using ArtMesh;
using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>
    /// Привязывает деталь к месту на меше ArtMesh: ищет треугольник под текущей позицией детали (в деформированной форме)
    /// и записывает барицентрические координаты. Перепривязка находит место по позиции в базовой форме.
    /// </summary>
    public static class DetailBinder
    {
        /// <summary>Привязка к точке меша, над которой сейчас стоит деталь (в осях ArtMesh-детали).</summary>
        public static bool BindAtDetailPosition(ArtDetail d, out string error)
        {
            if (!Prepare(d, out ArtMeshAsset a, out ArtMeshGeometry g, out ArtMeshState st, out error)) return false;

            Vector3 lp = d.Target.transform.InverseTransformPoint(d.transform.position);
            var pos = new Vector2[g.VertexCount];
            for (int i = 0; i < pos.Length; i++)
            {
                Vector3 f = ArtMeshEvaluator.FinalPos(a, g, st, i);
                pos[i] = new Vector2(f.x, f.y);
            }

            if (!TriangleLocator.Locate(pos, g.triangles, new Vector2(lp.x, lp.y), out int tri, out Vector3 bary))
            {
                error = "Не удалось найти треугольник под деталью.";
                return false;
            }
            Write(d, g, tri, bary);
            d.Refresh();
            return true;
        }

        /// <summary>Перепривязка после перепекания меша: место ищется по позиции в базовой форме.</summary>
        public static bool Rebind(ArtDetail d, out string error)
        {
            if (!Prepare(d, out ArtMeshAsset a, out ArtMeshGeometry g, out ArtMeshState st, out error)) return false;
            if (!d.Binding.isBound) { error = "Деталь не была привязана."; return false; }

            var pos = new Vector2[g.VertexCount];
            for (int i = 0; i < pos.Length; i++) pos[i] = g.basePos[i];

            if (!TriangleLocator.Locate(pos, g.triangles, d.Binding.basePosition, out int tri, out Vector3 bary))
            {
                error = "Не удалось найти место привязки в новом меше.";
                return false;
            }
            Write(d, g, tri, bary);
            d.Refresh();
            return true;
        }

        public static void Unbind(ArtDetail d)
        {
            Undo.RecordObject(d, "Unbind detail");
            d.Binding.isBound = false;
            EditorUtility.SetDirty(d);
            d.Refresh();
        }

        static bool Prepare(ArtDetail d, out ArtMeshAsset a, out ArtMeshGeometry g, out ArtMeshState st, out string error)
        {
            a = null; g = null; st = null; error = null;
            if (d == null || d.Target == null) { error = "У детали не задана цель (ArtMeshRenderer)."; return false; }
            a = d.Target.Asset;
            if (a == null || a.mesh == null) { error = "У цели нет запечённого меша."; return false; }
            g = a.Geometry;
            if (g == null || !g.IsValid) { error = "Меш цели нечитаем (включи Read/Write у меша)."; return false; }

            d.Target.Refresh();
            st = d.Target.State;
            if (st.lattice.Length != a.LatticeCount || st.vertexDelta.Length != g.VertexCount)
            {
                error = "Состояние цели не рассчитано (нет материала у цели?).";
                return false;
            }
            return true;
        }

        static void Write(ArtDetail d, ArtMeshGeometry g, int tri, Vector3 bary)
        {
            Undo.RecordObject(d, "Bind detail");
            var b = d.Binding;
            b.isBound = true;
            b.triangle = tri;
            b.barycentric = bary;
            b.vertexCount = g.VertexCount;
            Vector2 p0 = g.basePos[g.triangles[tri * 3]], p1 = g.basePos[g.triangles[tri * 3 + 1]], p2 = g.basePos[g.triangles[tri * 3 + 2]];
            b.basePosition = bary.x * p0 + bary.y * p1 + bary.z * p2;
            EditorUtility.SetDirty(d);
        }
    }
}
