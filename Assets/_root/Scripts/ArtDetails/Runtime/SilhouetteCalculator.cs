using ArtMesh;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Силуэт ArtMesh как многоугольник в осях детали: итоговые позиции вершин внешнего кольца радиального меша.
    /// Считается каждый кадр, поэтому следует за деформацией (сетка, правки вершин, scale/offset видов).
    /// </summary>
    public sealed class SilhouetteCalculator
    {
        public const int MaxPoints = 128;    // ArtMesh допускает до 128 лучей

        /// <summary>Записывает точки в buffer, возвращает их число (0, если силуэт получить нельзя).</summary>
        public int Compute(ArtMeshRenderer target, Vector2[] buffer)
        {
            if (target == null) return 0;
            var asset = target.Asset;
            if (asset == null || asset.mesh == null) return 0;
            var g = asset.Geometry;
            var topology = asset.topology;
            if (g == null || !g.IsValid || !topology.IsRadialFor(g.VertexCount)) return 0;

            var state = target.State;
            if (state.lattice.Length != asset.LatticeCount || state.vertexDelta.Length != g.VertexCount) return 0;

            int n = Mathf.Min(topology.rays, buffer.Length);
            int first = 1 + (topology.rings - 1) * topology.rays;
            for (int k = 0; k < n; k++)
            {
                Vector3 p = ArtMeshEvaluator.FinalPos(asset, g, state, first + k);
                buffer[k] = new Vector2(p.x, p.y);
            }
            return n;
        }
    }
}
