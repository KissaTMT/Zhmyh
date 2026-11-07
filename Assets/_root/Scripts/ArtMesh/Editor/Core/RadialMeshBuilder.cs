using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Результат построения радиального меша (чистые данные, без Unity-объектов).</summary>
    public sealed class RadialMeshData
    {
        public Vector3[] vertices;   // базовая форма (z = 0), локальные единицы
        public Vector2[] normals;    // внешняя нормаль вершины (только внешнее кольцо, иначе 0): для обводки
        public int[] triangles;
        public Bounds bounds;
    }

    /// <summary>
    /// Строит радиальный меш по альфа-маске спрайта: якорь + R колец по N лучей. Единственная ответственность:
    /// спрайт → данные меша. Требования: у текстуры Read/Write, спрайт Full Rect, без упаковки в SpriteAtlas.
    /// </summary>
    public static class RadialMeshBuilder
    {
        public static int[] BuildTriangles(int n, int r)
        {
            var t = new List<int>();
            for (int k = 0; k < n; k++)
            {
                t.Add(0); t.Add(1 + k); t.Add(1 + (k + 1) % n);
            }
            for (int ring = 1; ring < r; ring++)
                for (int k = 0; k < n; k++)
                {
                    int a = 1 + (ring - 1) * n + k;
                    int b = 1 + (ring - 1) * n + (k + 1) % n;
                    int c = 1 + ring * n + k;
                    int d = 1 + ring * n + (k + 1) % n;
                    t.Add(a); t.Add(b); t.Add(c);
                    t.Add(b); t.Add(d); t.Add(c);
                }
            return t.ToArray();
        }

        // Позиции вершин в пикселях внутри прямоугольника спрайта (начало — левый нижний угол).
        static Vector2[] PixelLayout(Sprite s, ArtMeshBuildSettings st, ref Vector2 anchor, bool auto)
        {
            Rect rect = s.textureRect;
            int w = (int)rect.width, h = (int)rect.height;
            Color[] px;
            try { px = s.texture.GetPixels((int)rect.x, (int)rect.y, w, h); }
            catch (UnityException e)
            {
                throw new InvalidOperationException(
                    $"Текстура спрайта '{s.name}' не читаема. Включи Read/Write в Import Settings. {e.Message}");
            }

            float thr = st.alphaThreshold;
            bool Opaque(float x, float y)
            {
                int ix = (int)x, iy = (int)y;
                return ix >= 0 && iy >= 0 && ix < w && iy < h && px[iy * w + ix].a > thr;
            }

            if (auto)
            {
                double sx = 0, sy = 0; int cnt = 0;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        if (px[y * w + x].a > thr) { sx += x; sy += y; cnt++; }
                anchor = cnt > 0 ? new Vector2((float)(sx / cnt) / w, (float)(sy / cnt) / h) : new Vector2(.5f, .5f);
            }

            int N = st.rays, R = st.rings;
            Vector2 c = new Vector2(anchor.x * w, anchor.y * h);
            float maxR = Mathf.Sqrt(w * w + h * h);
            var dir = new Vector2[N];
            var rim = new float[N];

            for (int k = 0; k < N; k++)
            {
                float a = Mathf.PI * 0.5f - k * 2f * Mathf.PI / N;   // старт сверху, по часовой
                dir[k] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float r = 0;
                for (float q = 0; q < maxR; q += 0.5f)
                    if (Opaque(c.x + dir[k].x * q, c.y + dir[k].y * q)) r = q;
                rim[k] = Mathf.Max(r, 1f);
            }

            for (int pass = 0; pass < st.rimSmooth; pass++)
            {
                var tmp = new float[N];
                for (int k = 0; k < N; k++)
                    tmp[k] = (rim[(k + N - 1) % N] + 2f * rim[k] + rim[(k + 1) % N]) * 0.25f;
                rim = tmp;
            }

            var p = new Vector2[1 + N * R];
            p[0] = c;
            for (int ring = 1; ring <= R; ring++)
            {
                float f = Mathf.Pow((float)ring / R, st.ringBias);
                for (int k = 0; k < N; k++)
                {
                    Vector2 v = c + dir[k] * (rim[k] * f);
                    v.x = Mathf.Clamp(v.x, 0, w); v.y = Mathf.Clamp(v.y, 0, h);
                    p[1 + (ring - 1) * N + k] = v;
                }
            }
            return p;
        }

        // Внешние нормали внешнего кольца. Обход по часовой, значит наружу = (−t.y, t.x).
        static Vector2[] OuterNormals(Vector2[] local, int n, int r)
        {
            var res = new Vector2[local.Length];
            int first = 1 + (r - 1) * n;
            for (int k = 0; k < n; k++)
            {
                Vector2 t = local[first + (k + 1) % n] - local[first + (k + n - 1) % n];
                Vector2 nr = new Vector2(-t.y, t.x);
                res[first + k] = nr.sqrMagnitude > 1e-12f ? nr.normalized : Vector2.zero;
            }
            return res;
        }

        public static RadialMeshData Build(Sprite sprite, ArtMeshBuildSettings st, ref Vector2 anchor, bool autoAnchor)
        {
            var pa = PixelLayout(sprite, st, ref anchor, autoAnchor);
            int cnt = pa.Length;

            var la = new Vector2[cnt];
            var d = new RadialMeshData
            {
                vertices = new Vector3[cnt],
                triangles = BuildTriangles(st.rays, st.rings)
            };

            float hw = 0f, minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < cnt; i++)
            {
                la[i] = (pa[i] - sprite.pivot) / sprite.pixelsPerUnit;   // пивот спрайта = начало координат
                d.vertices[i] = new Vector3(la[i].x, la[i].y, 0f);
                hw = Mathf.Max(hw, Mathf.Abs(la[i].x));
                minY = Mathf.Min(minY, la[i].y);
                maxY = Mathf.Max(maxY, la[i].y);
            }
            d.normals = OuterNormals(la, st.rays, st.rings);

            var bd = new Bounds();
            bd.SetMinMax(new Vector3(-hw, minY, -0.1f), new Vector3(hw, maxY, 0.1f));
            bd.Expand(bd.size.magnitude);   // запас под scale/offset/глубину видов и обводку
            d.bounds = bd;
            return d;
        }
    }
}
