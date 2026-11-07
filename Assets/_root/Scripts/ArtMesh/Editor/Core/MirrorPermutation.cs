using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Перестановка вершин «зеркальная соседка» (только редактор, нужна для создания зеркальных видов).
    /// Лучи идут от верха по часовой: угол a_k = 90° − 360°·k/N, отражение по X даёт a_(N−k). Со сдвигом: луч (shift − k)
    /// в том же кольце. Сдвиг подбирается так, чтобы соседка каждой вершины была ближе всего к её зеркальному образу.
    /// Перестановка — инволюция и сохраняет порядок обхода колец, поэтому зеркальная форма не выворачивается.
    /// </summary>
    public static class MirrorPermutation
    {
        public static int Index(int i, int n, int shift)
        {
            if (i <= 0) return 0;
            int k = (i - 1) % n, ring = (i - 1) / n;
            return 1 + ring * n + (((shift - k) % n) + n) % n;
        }

        /// <summary>partner[i] = индекс зеркальной соседки вершины i, либо null (и сообщение), если меш не радиальный.</summary>
        public static int[] Build(ArtMeshAsset asset, out string error)
        {
            error = null;
            var g = asset != null ? asset.Geometry : null;
            if (g == null || !g.IsValid) { error = "Меш не запечён."; return null; }
            var t = asset.topology;
            if (!t.IsRadialFor(g.VertexCount)) { error = "Топология меша не радиальная (сделай Bake заново)."; return null; }

            int n = t.rays;
            var pos = new Vector2[g.VertexCount];
            for (int i = 0; i < pos.Length; i++) pos[i] = g.basePos[i];

            int shift = FindShift(pos, n);
            var partner = new int[pos.Length];
            for (int i = 0; i < partner.Length; i++) partner[i] = Index(i, n, shift);
            return partner;
        }

        static int FindShift(Vector2[] pos, int n)
        {
            int best = 0;
            float bestE = float.MaxValue;
            for (int s = 0; s < n; s++)
            {
                float e = 0f;
                for (int i = 1; i < pos.Length; i++)
                {
                    Vector2 m = new Vector2(-pos[i].x, pos[i].y);
                    e += (pos[Index(i, n, s)] - m).sqrMagnitude;
                }
                if (e < bestE) { bestE = e; best = s; }
            }
            return best;
        }
    }
}
