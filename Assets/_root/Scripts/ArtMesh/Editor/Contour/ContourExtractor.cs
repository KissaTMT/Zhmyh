using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Достаёт внешний контур самой большой связной области альфа-маски спрайта (Moore-трассировка границы).
    /// Дырки игнорируются. Требует Read/Write у текстуры.
    /// </summary>
    public static class ContourExtractor
    {
        // W, NW, N, NE, E, SE, S, SW — по часовой стрелке при y вверх (как в текстуре)
        static readonly int[] DX = { -1, -1, 0, 1, 1, 1, 0, -1 };
        static readonly int[] DY = { 0, 1, 1, 1, 0, -1, -1, -1 };

        static int DirIndex(int dx, int dy)
        {
            for (int d = 0; d < 8; d++) if (DX[d] == dx && DY[d] == dy) return d;
            return 0;
        }

        public static Contour Extract(Sprite sp, float alphaThr = 0.2f, int samples = 512, int smoothPasses = 2)
        {
            if (sp == null) return null;
            Rect rect = sp.textureRect;
            int w = (int)rect.width, h = (int)rect.height;
            Color[] px;
            try { px = sp.texture.GetPixels((int)rect.x, (int)rect.y, w, h); }
            catch (UnityException e)
            {
                throw new InvalidOperationException(
                    $"Текстура спрайта '{sp.name}' не читаема. Включи Read/Write в Import Settings. {e.Message}");
            }

            // 1) связные компоненты (8-связность), берём самую большую
            var lab = new int[w * h];
            int best = 0, bestSize = 0, cur = 0;
            var stack = new Stack<int>();
            for (int i = 0; i < w * h; i++)
            {
                if (px[i].a <= alphaThr || lab[i] != 0) continue;
                cur++;
                int size = 0;
                lab[i] = cur;
                stack.Push(i);
                while (stack.Count > 0)
                {
                    int c = stack.Pop();
                    size++;
                    int cx = c % w, cy = c / w;
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = cx + DX[d], ny = cy + DY[d];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        int ni = ny * w + nx;
                        if (lab[ni] != 0 || px[ni].a <= alphaThr) continue;
                        lab[ni] = cur;
                        stack.Push(ni);
                    }
                }
                if (size > bestSize) { bestSize = size; best = cur; }
            }
            if (best == 0) return null;

            bool Fg(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && lab[y * w + x] == best;

            // 2) стартовый пиксель: самый нижний, затем самый левый
            int sx = -1, sy = -1;
            for (int y = 0; y < h && sx < 0; y++)
                for (int x = 0; x < w; x++)
                    if (lab[y * w + x] == best) { sx = x; sy = y; break; }

            // 3) Moore-трассировка границы
            var trace = new List<Vector2Int> { new Vector2Int(sx, sy) };
            int cx2 = sx, cy2 = sy, back = 0;           // back: направление на пиксель-«вход» (запад: слева пусто)
            int guard = w * h * 8;
            while (guard-- > 0)
            {
                int found = -1;
                for (int i = 1; i <= 8; i++)
                {
                    int d = (back + i) % 8;
                    if (Fg(cx2 + DX[d], cy2 + DY[d])) { found = d; break; }
                }
                if (found < 0) break;                    // одиночный пиксель

                int pd = (found + 7) % 8;                // фоновый сосед перед найденным
                int bx = cx2 + DX[pd], by = cy2 + DY[pd];
                int nx = cx2 + DX[found], ny = cy2 + DY[found];
                back = DirIndex(bx - nx, by - ny);
                cx2 = nx; cy2 = ny;
                if (cx2 == sx && cy2 == sy) break;
                trace.Add(new Vector2Int(cx2, cy2));
            }

            int M = trace.Count;
            if (M < 3) return null;

            var pts = new Vector2[M];
            for (int i = 0; i < M; i++) pts[i] = new Vector2(trace[i].x + 0.5f, trace[i].y + 0.5f);

            // 4) сглаживание ступенек пикселей
            for (int pass = 0; pass < smoothPasses; pass++)
            {
                var t = new Vector2[M];
                for (int i = 0; i < M; i++)
                    t[i] = (pts[(i + M - 2) % M] + pts[(i + M - 1) % M] + pts[i] + pts[(i + 1) % M] + pts[(i + 2) % M]) / 5f;
                pts = t;
            }

            // 5) равномерная передискретизация по длине дуги
            var cum = new float[M + 1];
            for (int i = 0; i < M; i++) cum[i + 1] = cum[i] + Vector2.Distance(pts[i], pts[(i + 1) % M]);
            float L = cum[M];
            if (L < 1e-6f) return null;

            var res = new Vector2[samples];
            int seg = 0;
            for (int j = 0; j < samples; j++)
            {
                float s = j * L / samples;
                while (seg < M - 1 && cum[seg + 1] < s) seg++;
                float segLen = cum[seg + 1] - cum[seg];
                float t = segLen > 1e-9f ? (s - cum[seg]) / segLen : 0f;
                res[j] = Vector2.Lerp(pts[seg], pts[(seg + 1) % M], t);
            }

            // 6) ориентация: по часовой (в y-вверх это отрицательная площадь)
            float area = 0f;
            for (int i = 0; i < samples; i++)
            {
                Vector2 a = res[i], b = res[(i + 1) % samples];
                area += a.x * b.y - b.x * a.y;
            }
            if (area > 0f) Array.Reverse(res);

            // 7) в локальные единицы меша (относительно пивота)
            for (int i = 0; i < samples; i++) res[i] = (res[i] - sp.pivot) / sp.pixelsPerUnit;
            return new Contour(res);
        }
    }
}
