using ArtMesh;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Считает точку привязки в осях ArtMesh-детали: барицентрическая комбинация итоговых (деформированных) позиций
    /// вершин привязанного треугольника. Чистая функция; ArtMesh только читается.
    /// </summary>
    public static class ArtDetailAnchorResolver
    {
        public static bool TryResolve(ArtMeshRenderer target, ArtDetailBinding binding, out Vector3 anchor)
        {
            anchor = Vector3.zero;
            if (target == null || binding == null || !binding.isBound) return false;

            var asset = target.Asset;
            if (asset == null || asset.mesh == null) return false;
            var g = asset.Geometry;
            if (g == null || !g.IsValid || binding.vertexCount != g.VertexCount) return false;

            int t = binding.triangle * 3;
            if (binding.triangle < 0 || t + 2 >= g.triangles.Length) return false;

            var state = target.State;
            if (state.lattice.Length != asset.LatticeCount || state.vertexDelta.Length != g.VertexCount) return false;   // состояние ещё не рассчитано

            Vector3 b = binding.barycentric;
            anchor = b.x * ArtMeshEvaluator.FinalPos(asset, g, state, g.triangles[t])
                   + b.y * ArtMeshEvaluator.FinalPos(asset, g, state, g.triangles[t + 1])
                   + b.z * ArtMeshEvaluator.FinalPos(asset, g, state, g.triangles[t + 2]);
            return true;
        }
    }
}
