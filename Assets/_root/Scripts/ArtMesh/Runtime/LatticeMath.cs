using UnityEngine;

namespace ArtMesh
{
    /// <summary>Бикубическая (Catmull-Rom) сетка деформации. Формулы совпадают с шейдером.</summary>
    public static class LatticeMath
    {
        public const int MaxDim = 8;   // 8x8 = 64 точек, размер массива в шейдере

        static Vector4 CR(float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return new Vector4(
                -0.5f * t3 + t2 - 0.5f * t,
                1.5f * t3 - 2.5f * t2 + 1f,
                -1.5f * t3 + 2f * t2 + 0.5f * t,
                0.5f * t3 - 0.5f * t2);
        }

        /// <summary>Смещение (xyz) в точке p плоскости XY. Сетка фиксирована: min..min+size.</summary>
        public static Vector3 Eval(Vector3[] d, int cols, int rows, Vector2 min, Vector2 size, Vector2 p)
        {
            if (d == null || d.Length < cols * rows || size.x < 1e-5f || size.y < 1e-5f) return Vector3.zero;
            float gx = Mathf.Clamp((p.x - min.x) / size.x * (cols - 1), 0f, cols - 1 - 1e-3f);
            float gy = Mathf.Clamp((p.y - min.y) / size.y * (rows - 1), 0f, rows - 1 - 1e-3f);
            int cx = (int)gx, cy = (int)gy;
            Vector4 wx = CR(gx - cx), wy = CR(gy - cy);
            Vector3 r = Vector3.zero;
            for (int j = 0; j < 4; j++)
            {
                int jj = Mathf.Clamp(cy - 1 + j, 0, rows - 1);
                Vector3 row = Vector3.zero;
                for (int i = 0; i < 4; i++)
                {
                    int ii = Mathf.Clamp(cx - 1 + i, 0, cols - 1);
                    row += wx[i] * d[jj * cols + ii];
                }
                r += wy[j] * row;
            }
            return r;
        }

        /// <summary>Билинейная передискретизация смещений при смене разрешения сетки.</summary>
        public static Vector3[] Resample(Vector3[] src, int sc, int sr, int dc, int dr)
        {
            var dst = new Vector3[dc * dr];
            for (int j = 0; j < dr; j++)
                for (int i = 0; i < dc; i++)
                {
                    float u = dc > 1 ? i / (float)(dc - 1) * (sc - 1) : 0f;
                    float v = dr > 1 ? j / (float)(dr - 1) * (sr - 1) : 0f;
                    int x0 = Mathf.Min((int)u, sc - 1), y0 = Mathf.Min((int)v, sr - 1);
                    int x1 = Mathf.Min(x0 + 1, sc - 1), y1 = Mathf.Min(y0 + 1, sr - 1);
                    float fx = u - x0, fy = v - y0;
                    dst[j * dc + i] = Vector3.Lerp(
                        Vector3.Lerp(src[y0 * sc + x0], src[y0 * sc + x1], fx),
                        Vector3.Lerp(src[y1 * sc + x0], src[y1 * sc + x1], fx), fy);
                }
            return dst;
        }
    }
}
