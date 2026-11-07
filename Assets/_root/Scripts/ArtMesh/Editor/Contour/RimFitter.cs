using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Подгонка внешнего кольца (вершины по кругу, по часовой) под контур. Чистая функция.
    /// Закреплённые вершины притягиваются к ближайшей точке контура (если snapPinned), остальные (если redistribute)
    /// равномерно по длине дуги расставляются на свободных участках между соседними закреплёнными с сохранением порядка.
    /// </summary>
    public static class RimFitter
    {
        public static Vector2[] Fit(Vector2[] rim, bool[] pinned, Contour c, bool snapPinned, bool redistribute)
        {
            int N = rim.Length;
            var res = (Vector2[])rim.Clone();

            var idx = new List<int>();
            for (int k = 0; k < N; k++) if (pinned[k]) idx.Add(k);
            int m = idx.Count;
            if (m == 0) return res;

            float L = c.Length;
            var s = new float[m];
            for (int j = 0; j < m; j++) s[j] = c.Project(rim[idx[j]], out _, out _);
            var u = ContourArcs.Unwrap(s, L);

            for (int j = 0; j < m; j++)
            {
                int ka = idx[j];
                if (snapPinned) res[ka] = c.Sample(u[j]);
                if (!redistribute) continue;

                int kb = idx[(j + 1) % m];
                float sa = u[j];
                float sb = (j < m - 1) ? u[j + 1] : u[0] + L;
                if (sb < sa) sb = sa;

                int q = ((kb - ka - 1) % N + N) % N;     // число свободных вершин между двумя закреплёнными
                for (int t = 1; t <= q; t++)
                    res[(ka + t) % N] = c.Sample(sa + (sb - sa) * t / (q + 1f));
            }
            return res;
        }
    }
}
