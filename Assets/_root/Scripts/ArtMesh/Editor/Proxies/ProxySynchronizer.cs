using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Двусторонняя синхронизация прокси ↔ данные вида. Синхронизация абсолютная (смещение = позиция − база),
    /// поэтому любые инструменты Unity и Undo/Redo работают без особой логики.
    ///   • Корень прокси = трансформ вида: Position → offset (Z = глубина), Scale → scale. Поворот сбрасывается.
    ///   • Точки: полное 3D-смещение (Z = глубина точки).
    /// </summary>
    public static class ProxySynchronizer
    {
        /// <summary>Читает положения прокси и корня и записывает в вид. true, если данные изменились.</summary>
        public static bool Sync(ProxySession s)
        {
            if (!s.IsAlive) return false;
            var r = s.renderer; var a = s.Asset; var v = s.View; var deltas = s.Deltas;
            int n = s.points.Length;
            if (s.kind == ProxyKind.Lattice && s.cols * s.rows != n) return false;
            if (s.kind == ProxyKind.Vertices)
            {
                var g = a.Geometry;
                if (g == null || !g.IsValid) return false;
            }

            bool dirty = false;
            Transform root = s.root.transform;

            // 1) корень = трансформ вида
            if (root.localRotation != Quaternion.identity) root.localRotation = Quaternion.identity;
            Vector3 sc = root.localScale;
            for (int c = 0; c < 3; c++)
                if (Mathf.Abs(sc[c]) < 1e-3f) sc[c] = sc[c] < 0f ? -1e-3f : 1e-3f;
            if (sc != root.localScale) root.localScale = sc;
            if ((v.scale - sc).sqrMagnitude > 1e-10f || (v.offset - root.localPosition).sqrMagnitude > 1e-10f)
            {
                Undo.RecordObject(a, "Edit ArtMesh view transform");
                v.scale = sc;
                v.offset = root.localPosition;
                dirty = true;
            }

            // 2) точки: смещение = локальная позиция − база − уже записанное
            var moved = new List<int>();
            var disp = new List<Vector3>();
            var bases = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                var t = s.points[i];
                if (t == null) return false;               // прокси удалили руками
                bases[i] = ProxyGeometry.Base(s, i);
                Vector3 d = t.localPosition - bases[i] - deltas[i];
                if (d.sqrMagnitude > 1e-12f) { moved.Add(i); disp.Add(d); }
            }

            var touched = new HashSet<int>(moved);
            if (moved.Count > 0)
            {
                Undo.RecordObject(a, "Edit ArtMesh view");

                float radius = SoftFalloff.RadiusFor(s);
                bool soft = radius > 1e-5f && (long)n * moved.Count < 3000000L;

                for (int m = 0; m < moved.Count; m++) deltas[moved[m]] += disp[m];
                if (soft) SoftFalloff.Apply(s, moved, disp, deltas, touched, radius);
                dirty = true;
            }

            // точки, которых коснулось мягкое влияние, нужно переставить
            foreach (int i in touched) s.points[i].localPosition = bases[i] + deltas[i];

            if (dirty)
            {
                EditorUtility.SetDirty(a);
                r.Refresh();
            }
            return dirty;
        }

        /// <summary>Расставляет корень и все точки по данным вида (после Undo, внешних операций, смены параметров вида).</summary>
        public static void Reposition(ProxySession s)
        {
            if (!s.IsAlive) return;
            s.renderer.Refresh();
            ProxyGeometry.PlaceRoot(s);
            for (int i = 0; i < s.points.Length; i++) ProxyGeometry.Place(s, i);
        }

        /// <summary>Фиксирует изменения данных, сделанные программно (подгонка по контуру и т.п.).</summary>
        public static void Commit(ProxySession s)
        {
            EditorUtility.SetDirty(s.Asset);
            Reposition(s);
        }
    }
}
