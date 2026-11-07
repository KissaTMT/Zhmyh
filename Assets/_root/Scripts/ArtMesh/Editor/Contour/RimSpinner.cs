using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// «Прокрутка» вершин внешнего кольца по контуру. Чистая функция: результат целиком зависит от входных позиций
    /// и величины spin (в слотах, можно дробную), поэтому её можно вызывать повторно от одного и того же снимка.
    ///
    /// Без блокировок: всё кольцо циклически сдвигается вдоль контура, сохраняя текущее неравномерное распределение
    /// (вершина k занимает место вершины k + spin, между слотами интерполяция).
    ///
    /// С блокировками: заблокированные вершины стоят на месте. Свободные вершины каждого участка между двумя заблокированными
    /// равномерно распределяются по свободному куску контура и сдвигаются по нему на spin слотов. Порядок вершин не
    /// нарушается: вершина, дошедшая до края участка, прижимается к заблокированной соседке (мягкое сжатие), но не проходит
    /// сквозь неё. Иначе треугольники кольца провернулись бы через заблокированный участок.
    /// </summary>
    public static class RimSpinner
    {
        public static Vector2[] Spin(Vector2[] rim, bool[] locked, Contour c, float spin)
        {
            int N = rim.Length;
            var li = new List<int>();
            for (int k = 0; k < N; k++) if (locked[k]) li.Add(k);

            if (li.Count == 0) return SpinFree(rim, c, spin);
            if (li.Count == N) return (Vector2[])rim.Clone();
            return SpinWithLocks(rim, li, c, spin, N);
        }

        // ---- кольцо без блокировок: циклический сдвиг текущего распределения
        static Vector2[] SpinFree(Vector2[] rim, Contour c, float spin)
        {
            int N = rim.Length;
            float L = c.Length;
            var s = new float[N];
            for (int k = 0; k < N; k++) s[k] = c.Project(rim[k], out _, out _);
            var u = ContourArcs.Unwrap(s, L);

            float U(int i)
            {
                int lap = Mathf.FloorToInt(i / (float)N);
                return u[i - lap * N] + lap * L;
            }

            var res = new Vector2[N];
            for (int k = 0; k < N; k++)
            {
                float x = k + spin;
                int i0 = Mathf.FloorToInt(x);
                float t = x - i0;
                res[k] = c.Sample(Mathf.Lerp(U(i0), U(i0 + 1), t));
            }
            return res;
        }

        // ---- есть блокировки: свободные участки между заблокированными
        static Vector2[] SpinWithLocks(Vector2[] rim, List<int> li, Contour c, float spin, int N)
        {
            int m = li.Count;
            float L = c.Length;
            var res = (Vector2[])rim.Clone();

            var s = new float[m];
            for (int j = 0; j < m; j++) s[j] = c.Project(rim[li[j]], out _, out _);
            var u = ContourArcs.Unwrap(s, L);

            for (int j = 0; j < m; j++)
            {
                int ka = li[j], kb = li[(j + 1) % m];
                float sa = u[j];
                float sb = (j < m - 1) ? u[j + 1] : u[0] + L;
                if (sb < sa) sb = sa;

                int q = ((kb - ka - 1) % N + N) % N;       // свободных вершин на участке
                for (int t = 1; t <= q; t++)
                {
                    float g = SoftSlide(t + spin, q);       // (0, q+1)
                    res[(ka + t) % N] = c.Sample(sa + (sb - sa) * g / (q + 1f));
                }
            }
            return res;
        }

        /// <summary>
        /// Монотонное отображение прямой в (0, q+1): тождество на [1, q], за краями плавно сжимается к 0 и q+1
        /// (производная непрерывна). Нужно, чтобы вершины прижимались к заблокированным соседям, не проходя сквозь них.
        /// </summary>
        public static float SoftSlide(float x, int q)
        {
            if (x < 1f) return Mathf.Exp(x - 1f);
            if (x > q) return q + 1f - Mathf.Exp(-(x - q));
            return x;
        }
    }
}
