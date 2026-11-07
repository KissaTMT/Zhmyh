using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>Поиск треугольника, в котором лежит точка, и её барицентрических координат. Чистая функция.</summary>
    public static class TriangleLocator
    {
        /// <summary>
        /// Ищет треугольник, содержащий p. Если точка вне меша, берётся ближайший (с зажатыми и перенормированными
        /// координатами). Возвращает false, только если вырожденная геометрия.
        /// </summary>
        public static bool Locate(Vector2[] pos, int[] tris, Vector2 p, out int triangle, out Vector3 bary)
        {
            triangle = -1;
            bary = Vector3.zero;
            float best = float.NegativeInfinity;

            for (int t = 0; t + 2 < tris.Length; t += 3)
            {
                if (!Barycentric(pos[tris[t]], pos[tris[t + 1]], pos[tris[t + 2]], p, out Vector3 b)) continue;
                float m = Mathf.Min(b.x, Mathf.Min(b.y, b.z));          // ≥ 0 = внутри
                if (m > best)
                {
                    best = m; triangle = t / 3; bary = b;
                    if (m >= -1e-6f) break;                              // нашли содержащий треугольник
                }
            }
            if (triangle < 0) return false;

            bary = new Vector3(Mathf.Max(bary.x, 0f), Mathf.Max(bary.y, 0f), Mathf.Max(bary.z, 0f));
            float s = bary.x + bary.y + bary.z;
            bary = s > 1e-9f ? bary / s : new Vector3(1f, 0f, 0f);
            return true;
        }

        public static bool Barycentric(Vector2 a, Vector2 b, Vector2 c, Vector2 p, out Vector3 bary)
        {
            Vector2 v0 = b - a, v1 = c - a, v2 = p - a;
            float d00 = Vector2.Dot(v0, v0), d01 = Vector2.Dot(v0, v1), d11 = Vector2.Dot(v1, v1);
            float d20 = Vector2.Dot(v2, v0), d21 = Vector2.Dot(v2, v1);
            float den = d00 * d11 - d01 * d01;
            if (Mathf.Abs(den) < 1e-12f) { bary = Vector3.zero; return false; }
            float v = (d11 * d20 - d01 * d21) / den;
            float w = (d00 * d21 - d01 * d20) / den;
            bary = new Vector3(1f - v - w, v, w);
            return true;
        }
    }
}
