using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Замкнутый контур: точки равномерно по длине дуги, по часовой стрелке (y вверх),
    /// в локальных единицах меша (относительно пивота спрайта). Только геометрия.
    /// </summary>
    public sealed class Contour
    {
        public readonly Vector2[] pts;
        readonly float[] cum;                 // cum[i] = длина дуги до pts[i], cum[M] = полная длина
        public float Length => cum[pts.Length];

        public Contour(Vector2[] p)
        {
            pts = p;
            cum = new float[p.Length + 1];
            for (int i = 0; i < p.Length; i++)
                cum[i + 1] = cum[i] + Vector2.Distance(p[i], p[(i + 1) % p.Length]);
        }

        /// <summary>Точка контура по параметру длины дуги (циклический).</summary>
        public Vector2 Sample(float s)
        {
            float L = Length;
            s = Mathf.Repeat(s, L);
            int idx = System.Array.BinarySearch(cum, s);
            if (idx < 0) idx = ~idx - 1;
            idx = Mathf.Clamp(idx, 0, pts.Length - 1);
            float seg = cum[idx + 1] - cum[idx];
            float t = seg > 1e-9f ? (s - cum[idx]) / seg : 0f;
            return Vector2.Lerp(pts[idx], pts[(idx + 1) % pts.Length], t);
        }

        /// <summary>Ближайшая точка контура. Возвращает параметр длины дуги.</summary>
        public float Project(Vector2 p, out Vector2 closest, out float dist)
        {
            float bestD = float.MaxValue, bestS = 0f;
            Vector2 bestC = pts[0];
            for (int i = 0; i < pts.Length; i++)
            {
                Vector2 a = pts[i], b = pts[(i + 1) % pts.Length];
                Vector2 ab = b - a;
                float l2 = ab.sqrMagnitude;
                float t = l2 > 1e-12f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / l2) : 0f;
                Vector2 q = a + ab * t;
                float d = (p - q).sqrMagnitude;
                if (d < bestD) { bestD = d; bestC = q; bestS = cum[i] + t * (cum[i + 1] - cum[i]); }
            }
            closest = bestC;
            dist = Mathf.Sqrt(bestD);
            return bestS;
        }

        /// <summary>Отражение по X. Порядок точек разворачивается, чтобы контур остался по часовой.</summary>
        public Contour Mirrored()
        {
            var m = new Vector2[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                Vector2 q = pts[pts.Length - 1 - i];
                m[i] = new Vector2(-q.x, q.y);
            }
            return new Contour(m);
        }
    }
}
