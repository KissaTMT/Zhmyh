using System;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Интерактивная прокрутка кольца по контуру. При старте снимает снимок правок вершин и позиций кольца;
    /// каждое Apply(spin) считается от этого снимка, поэтому ползунок идемпотентен и обратим. Cancel возвращает снимок.
    /// Единственная ответственность: применить результат RimSpinner к данным вида.
    /// </summary>
    public sealed class RimSpinSession
    {
        readonly ProxySession session;
        readonly Contour contour;
        readonly bool propagateInterior;
        readonly Vector3[] snapshot;
        readonly Vector2[] rim0;
        readonly bool[] locked;
        readonly int n, rings, first;

        public int RimCount => n;
        public int LockedCount { get; }

        RimSpinSession(ProxySession s, Contour c, bool propagate, Vector3[] snap, Vector2[] r0, bool[] lk, int rays, int rngs)
        {
            session = s; contour = c; propagateInterior = propagate; snapshot = snap; rim0 = r0; locked = lk;
            n = rays; rings = rngs; first = 1 + (rngs - 1) * rays;
            foreach (bool b in lk) if (b) LockedCount++;
        }

        public static RimSpinSession Begin(ProxySession s, Contour c, ContourFitOptions o, out string error)
        {
            error = null;
            if (s == null || !s.IsAlive || s.kind != ProxyKind.Vertices) { error = "Нужны прокси вершин."; return null; }
            var a = s.Asset; var g = a.Geometry; var v = s.View; var st = s.renderer.State;
            if (g == null || !g.IsValid) { error = "Меш не содержит данных ArtMesh. Сделай Bake заново."; return null; }
            int rings = ProxySelectionTools.Rings(s);
            if (rings == 0) { error = "Топология меша не радиальная."; return null; }

            int N = ProxySelectionTools.Rays(s);
            int first = 1 + (rings - 1) * N;
            var rim0 = new Vector2[N];
            var locked = new bool[N];
            for (int k = 0; k < N; k++)
            {
                rim0[k] = ViewTransform.ToDisplay(v, ArtMeshEvaluator.ShapeFinal(a, g, st, first + k));
                locked[k] = s.lockedVertices.Contains(first + k);
            }
            return new RimSpinSession(s, c, o.propagateInterior, (Vector3[])v.vertexDelta.Clone(), rim0, locked, N, rings);
        }

        /// <summary>Пересчитывает кольцо для сдвига spin (в слотах) от снимка и записывает в вид.</summary>
        public void Apply(float spin)
        {
            if (!session.IsAlive) return;
            var a = session.Asset; var v = session.View;
            var fitted = RimSpinner.Spin(rim0, locked, contour, spin);

            Undo.RecordObject(a, "Spin rim along contour");
            Array.Copy(snapshot, v.vertexDelta, snapshot.Length);
            for (int k = 0; k < n; k++)
            {
                Vector3 d = ViewTransform.ToShapeDelta(v, fitted[k] - rim0[k]);
                v.vertexDelta[first + k] += d;
                if (!propagateInterior) continue;
                for (int r = 1; r < rings; r++)
                    v.vertexDelta[1 + (r - 1) * n + k] += d * Mathf.Pow((float)r / rings, a.topology.ringBias);
            }
            ProxySynchronizer.Commit(session);
        }

        /// <summary>Возвращает правки вершин к моменту начала прокрутки.</summary>
        public void Cancel()
        {
            if (!session.IsAlive) return;
            Undo.RecordObject(session.Asset, "Cancel rim spin");
            Array.Copy(snapshot, session.View.vertexDelta, snapshot.Length);
            ProxySynchronizer.Commit(session);
        }
    }
}
