using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// CPU-аналог вершинного шейдера. Конвейер: база → сетка → правки вершин → scale/offset.
    /// Чистые функции без зависимостей от компонентов.
    /// </summary>
    public static class ArtMeshEvaluator
    {
        /// <summary>Форма после сетки, до правок вершин и до трансформа вида (пространство прокси).</summary>
        public static Vector3 ShapeBase(ArtMeshAsset a, ArtMeshGeometry g, ArtMeshState s, int i)
        {
            Vector2 p = g.basePos[i];
            Vector3 q = new Vector3(p.x, p.y, 0f);
            if (a.useLattice) q += LatticeMath.Eval(s.lattice, a.latticeCols, a.latticeRows, a.latticeMin, a.latticeSize, p);
            return q;
        }

        /// <summary>Форма с правками вершин, до трансформа вида.</summary>
        public static Vector3 ShapeFinal(ArtMeshAsset a, ArtMeshGeometry g, ArtMeshState s, int i)
        {
            Vector3 q = ShapeBase(a, g, s, i);
            if (i < s.vertexDelta.Length) q += s.vertexDelta[i];
            return q;
        }

        /// <summary>Итоговая позиция в локальном пространстве детали (без обводки).</summary>
        public static Vector3 FinalPos(ArtMeshAsset a, ArtMeshGeometry g, ArtMeshState s, int i)
            => Vector3.Scale(ShapeFinal(a, g, s, i), s.scale) + s.offset;
    }
}
