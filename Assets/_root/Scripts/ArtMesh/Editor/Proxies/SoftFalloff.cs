using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Мягкое влияние: невыбранные точки в радиусе повторяют смещение ближайшей сдвинутой точки с затуханием.
    /// Расстояние топологическое (шаги по сетке или по кольцам и лучам меша), а не евклидово: во внутренних кольцах
    /// вершины стоят очень плотно, и мировой радиус утягивал бы целые кластеры.
    /// </summary>
    public static class SoftFalloff
    {
        public static float RadiusFor(ProxySession s)
            => s.kind == ProxyKind.Vertices ? s.options.vertexBrush : s.options.latticeBrush;

        /// <summary>Расстояние между точками a и b в шагах топологии.</summary>
        public static float Distance(ProxySession s, int a, int b)
        {
            if (a == b) return 0f;
            if (s.kind == ProxyKind.Lattice)
            {
                float dx = (a % s.cols) - (b % s.cols), dy = (a / s.cols) - (b / s.cols);
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            int n = s.Asset.topology.rays;
            if (n <= 0) return float.MaxValue;
            int ra = a == 0 ? 0 : (a - 1) / n + 1, rb = b == 0 ? 0 : (b - 1) / n + 1;
            if (a == 0 || b == 0) return Mathf.Abs(ra - rb);          // центр: расстояние = номер кольца другой вершины
            int dk = Mathf.Abs((a - 1) % n - (b - 1) % n);
            dk = Mathf.Min(dk, n - dk);                               // по лучам кольцо замкнуто
            int dr = Mathf.Abs(ra - rb);
            return Mathf.Sqrt(dr * dr + dk * dk);
        }

        /// <param name="moved">индексы сдвинутых точек</param>
        /// <param name="disp">их смещения</param>
        /// <param name="deltas">массив смещений вида; изменяется</param>
        /// <param name="touched">сюда добавляются затронутые соседи</param>
        public static void Apply(ProxySession s, List<int> moved, List<Vector3> disp, Vector3[] deltas,
                                 HashSet<int> touched, float radius)
        {
            int n = deltas.Length;
            for (int j = 0; j < n; j++)
            {
                if (touched.Contains(j)) continue;
                float md = float.MaxValue; int bi = -1;
                for (int m = 0; m < moved.Count; m++)
                {
                    float d = Distance(s, j, moved[m]);
                    if (d < md) { md = d; bi = m; }
                }
                if (bi < 0 || md >= radius) continue;
                deltas[j] += disp[bi] * Mathf.SmoothStep(1f, 0f, md / radius);
                touched.Add(j);
            }
        }
    }
}
