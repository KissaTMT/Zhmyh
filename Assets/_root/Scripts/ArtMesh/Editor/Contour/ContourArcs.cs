using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Работа с параметрами длины дуги закреплённых точек на замкнутом контуре. Чистые функции.</summary>
    public static class ContourArcs
    {
        /// <summary>
        /// По параметрам s точек (в порядке обхода кольца) строит неубывающую последовательность u в пределах одного витка L.
        /// Шаг назад (когда следующая точка по проекции «позади» предыдущей) схлопывается в ноль, порядок не нарушается.
        /// </summary>
        public static float[] Unwrap(float[] s, float L)
        {
            int m = s.Length;
            var f = new float[m];
            float sum = 0f;
            for (int j = 0; j < m; j++)
            {
                f[j] = Mathf.Repeat(s[(j + 1) % m] - s[j], L);
                sum += f[j];
            }
            if (m == 1) { f[0] = L; sum = L; }

            // порядок нарушен (шаг назад превратился в ~L): схлопываем такой шаг в ноль
            int guard = m;
            while (sum > L * 1.5f && guard-- > 0)
            {
                int jm = 0;
                for (int j = 1; j < m; j++) if (f[j] > f[jm]) jm = j;
                sum -= f[jm];
                f[jm] = 0f;
            }

            var u = new float[m];
            u[0] = s[0];
            for (int j = 1; j < m; j++) u[j] = u[j - 1] + f[j - 1];
            return u;
        }
    }
}
