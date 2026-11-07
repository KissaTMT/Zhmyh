using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Проверки целостности прокси. Только чтение и выделение.</summary>
    public static class ProxyDiagnostics
    {
        /// <summary>
        /// Находит точки, стоящие в одном месте (ближе eps), и выделяет их. Совпавшие вершины визуально выглядят
        /// как «один объект управляет одной вершиной», хотя это разные вершины в одной позиции.
        /// Причины: подгонка по контуру с нарушенным порядком, прокрутка с прижатием, схлопнутые лучи у якоря.
        /// </summary>
        public static string SelectCoincident(ProxySession s, float eps)
        {
            if (s == null || !s.IsAlive) return "Нет прокси.";
            int n = s.points.Length;
            var pos = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                if (s.points[i] == null) return "Прокси повреждены (объект удалён).";
                pos[i] = s.points[i].localPosition;
            }

            float e2 = eps * eps;
            var set = new HashSet<int>();
            int pairs = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if ((pos[i] - pos[j]).sqrMagnitude < e2) { set.Add(i); set.Add(j); pairs++; }

            if (set.Count == 0) return "Совпадающих вершин нет: каждый объект управляет своей вершиной.";
            ProxySelectionTools.Select(s, set);
            return $"Вершин в одной позиции: {set.Count} (пар: {pairs}). Они выделены.";
        }
    }
}
