using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Подгонка вершин по контуру (правки вершин вида). Контур сравнивается с формой после трансформа вида;
    /// глубина Z вершин не меняется. Возвращает сообщение для пользователя.
    /// </summary>
    public static class ContourFitService
    {
        static bool Check(ProxySession s, out string error)
        {
            error = null;
            if (s == null || !s.IsAlive || s.kind != ProxyKind.Vertices) { error = "Нужны прокси вершин."; return false; }
            var g = s.Asset.Geometry;
            if (g == null || !g.IsValid) { error = "Меш не содержит данных ArtMesh. Сделай Bake заново."; return false; }
            return true;
        }

        /// <summary>Каждую выбранную вершину переносит в ближайшую точку контура.</summary>
        public static string SnapSelected(ProxySession s, Contour c)
        {
            if (!Check(s, out string err)) return err;
            var sel = ProxySelectionTools.SelectedIndices(s);
            if (sel.Count == 0) return "Выбери вершины-прокси.";

            var a = s.Asset; var g = a.Geometry; var v = s.View; var st = s.renderer.State;
            Undo.RecordObject(a, "Snap vertices to contour");

            var cur = new Vector2[sel.Count];
            for (int m = 0; m < sel.Count; m++)
                cur[m] = ViewTransform.ToDisplay(v, ArtMeshEvaluator.ShapeFinal(a, g, st, sel[m]));
            for (int m = 0; m < sel.Count; m++)
            {
                c.Project(cur[m], out Vector2 q, out _);
                v.vertexDelta[sel[m]] += ViewTransform.ToShapeDelta(v, q - cur[m]);
            }
            ProxySynchronizer.Commit(s);
            return $"Привязано к контуру: {sel.Count}.";
        }

        /// <summary>
        /// Выбранные вершины внешнего кольца закрепляются (и притягиваются к контуру), остальные равномерно
        /// расставляются по ещё не занятому контуру. Внутренние кольца идут следом.
        /// </summary>
        public static string FitRim(ProxySession s, Contour c, ContourFitOptions o)
        {
            if (!Check(s, out string err)) return err;
            int rings = ProxySelectionTools.Rings(s);
            if (rings == 0) return "Топология меша не радиальная.";

            var a = s.Asset; var g = a.Geometry; var v = s.View; var st = s.renderer.State;
            int N = ProxySelectionTools.Rays(s);
            int first = 1 + (rings - 1) * N;

            var sel = new HashSet<int>(ProxySelectionTools.SelectedIndices(s));
            var pinned = new bool[N];
            int pc = 0;
            for (int k = 0; k < N; k++) if (sel.Contains(first + k)) { pinned[k] = true; pc++; }
            if (pc == 0) return "Выбери хотя бы одну вершину внешнего кольца.";

            var rim = new Vector2[N];
            for (int k = 0; k < N; k++)
                rim[k] = ViewTransform.ToDisplay(v, ArtMeshEvaluator.ShapeFinal(a, g, st, first + k));
            var fitted = RimFitter.Fit(rim, pinned, c, o.snapPinned, o.redistribute);

            Undo.RecordObject(a, "Fit rim to contour");
            for (int k = 0; k < N; k++)
            {
                Vector3 d = ViewTransform.ToShapeDelta(v, fitted[k] - rim[k]);
                v.vertexDelta[first + k] += d;
                if (!o.propagateInterior) continue;
                for (int r = 1; r < rings; r++)
                    v.vertexDelta[1 + (r - 1) * N + k] += d * Mathf.Pow((float)r / rings, a.topology.ringBias);
            }
            ProxySynchronizer.Commit(s);
            return $"Закреплено вершин: {pc}, расставлено остальных: {(o.redistribute ? N - pc : 0)}.";
        }

        /// <summary>Выделяет вершины внешнего кольца, которые не лежат на контуре (кандидаты на «лишние»).</summary>
        public static string SelectOffContour(ProxySession s, Contour c, float tolerance)
        {
            if (!Check(s, out string err)) return err;
            int rings = ProxySelectionTools.Rings(s);
            if (rings == 0) return "Топология меша не радиальная.";

            var a = s.Asset; var g = a.Geometry; var v = s.View; var st = s.renderer.State;
            int N = ProxySelectionTools.Rays(s);
            int first = 1 + (rings - 1) * N;

            var off = new List<int>();
            for (int k = 0; k < N; k++)
            {
                Vector2 p = ViewTransform.ToDisplay(v, ArtMeshEvaluator.ShapeFinal(a, g, st, first + k));
                c.Project(p, out _, out float dist);
                if (dist > tolerance) off.Add(first + k);
            }
            ProxySelectionTools.Select(s, off);
            return $"Вне контура: {off.Count} из {N}.";
        }
    }
}
