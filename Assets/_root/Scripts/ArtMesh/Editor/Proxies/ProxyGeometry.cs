using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Где должна стоять каждая прокси-точка: база (без смещения вида) + смещение. База считается в пространстве формы.</summary>
    public static class ProxyGeometry
    {
        /// <summary>База точки: узел сетки в регулярной решётке или вершина после зеркала вида и сетки (до правок вершин).</summary>
        public static Vector3 Base(ProxySession s, int i)
        {
            var a = s.Asset;
            if (s.kind == ProxyKind.Lattice)
            {
                int C = s.cols, R = s.rows;
                return new Vector3(a.latticeMin.x + (i % C) / (float)(C - 1) * a.latticeSize.x,
                                   a.latticeMin.y + (i / C) / (float)(R - 1) * a.latticeSize.y, 0f);
            }
            return ArtMeshEvaluator.ShapeBase(a, a.Geometry, s.renderer.State, i);
        }

        public static void Place(ProxySession s, int i)
        {
            s.points[i].localPosition = Base(s, i) + s.Deltas[i];
        }

        public static void PlaceRoot(ProxySession s)
        {
            var v = s.View;
            var t = s.root.transform;
            t.localPosition = v.offset;
            t.localRotation = Quaternion.identity;
            t.localScale = v.scale;
        }
    }
}
