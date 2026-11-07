using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Выбор прокси-точек: все, инверсия, строки и столбцы сетки, кольца и лучи вершин.</summary>
    public static class ProxySelectionTools
    {
        public static List<int> SelectedIndices(ProxySession s)
        {
            var set = new HashSet<Transform>(Selection.transforms);
            var l = new List<int>();
            if (s.points != null)
                for (int i = 0; i < s.points.Length; i++)
                    if (s.points[i] != null && set.Contains(s.points[i])) l.Add(i);
            return l;
        }

        public static void Select(ProxySession s, IEnumerable<int> indices)
        {
            var list = new List<UnityEngine.Object>();
            foreach (int i in indices)
                if (i >= 0 && i < s.points.Length && s.points[i] != null) list.Add(s.points[i].gameObject);
            Selection.objects = list.ToArray();
        }

        public static void All(ProxySession s) => Select(s, Range(0, s.points.Length));

        public static void Invert(ProxySession s)
        {
            var sel = new HashSet<int>(SelectedIndices(s));
            var inv = new List<int>();
            for (int i = 0; i < s.points.Length; i++) if (!sel.Contains(i)) inv.Add(i);
            Select(s, inv);
        }

        // ---- сетка
        public static void LatticeRow(ProxySession s, int row)
        {
            var l = new List<int>(); for (int i = 0; i < s.cols; i++) l.Add(row * s.cols + i);
            Select(s, l);
        }

        public static void LatticeColumn(ProxySession s, int col)
        {
            var l = new List<int>(); for (int j = 0; j < s.rows; j++) l.Add(j * s.cols + col);
            Select(s, l);
        }

        public static void LatticeBorder(ProxySession s)
        {
            var l = new List<int>();
            for (int j = 0; j < s.rows; j++)
                for (int i = 0; i < s.cols; i++)
                    if (i == 0 || j == 0 || i == s.cols - 1 || j == s.rows - 1) l.Add(j * s.cols + i);
            Select(s, l);
        }

        // ---- вершины радиального меша
        public static int Rays(ProxySession s) => s.Asset.topology.rays;

        /// <summary>Число колец или 0, если топология не радиальная.</summary>
        public static int Rings(ProxySession s)
        {
            int n = Rays(s);
            return (n > 0 && (s.points.Length - 1) % n == 0) ? (s.points.Length - 1) / n : 0;
        }

        public static void Center(ProxySession s) => Select(s, new[] { 0 });

        public static void Ring(ProxySession s, int ring)
        {
            int n = Rays(s), first = 1 + (ring - 1) * n;
            var l = new List<int>(); for (int k = 0; k < n; k++) l.Add(first + k);
            Select(s, l);
        }

        public static void Ray(ProxySession s, int ray)
        {
            int n = Rays(s), rings = Rings(s);
            var l = new List<int>(); for (int r = 1; r <= rings; r++) l.Add(1 + (r - 1) * n + ray);
            Select(s, l);
        }

        /// <summary>Дуга внешнего кольца от луча from до луча to включительно (по часовой, замкнуто).</summary>
        public static void RimArc(ProxySession s, int from, int to)
        {
            int n = Rays(s), rings = Rings(s);
            if (rings == 0) return;
            int first = 1 + (rings - 1) * n;
            int k = ((from % n) + n) % n, end = ((to % n) + n) % n;
            var l = new List<int>();
            for (int guard = 0; guard < n; guard++)
            {
                l.Add(first + k);
                if (k == end) break;
                k = (k + 1) % n;
            }
            Select(s, l);
        }

        public static void OuterRim(ProxySession s)
        {
            int rings = Rings(s);
            if (rings > 0) Ring(s, rings);
        }

        static IEnumerable<int> Range(int a, int b) { for (int i = a; i < b; i++) yield return i; }
    }
}
