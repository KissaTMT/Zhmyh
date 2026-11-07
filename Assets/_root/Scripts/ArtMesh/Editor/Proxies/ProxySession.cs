using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    public enum ProxyKind { Lattice, Vertices }

    /// <summary>Состояние одной сессии редактирования через GameObject-прокси. Только данные и доступ к ним.</summary>
    public sealed class ProxySession
    {
        public ArtMeshRenderer renderer;
        public int viewIndex;
        public ProxyKind kind;
        public GameObject root;           // корень = трансформ вида (Position → offset, Scale → scale)
        public Transform[] points;        // по одному объекту на узел сетки или вершину
        public int cols, rows;            // разрешение сетки на момент создания
        public ProxyOptions options;

        /// <summary>Заблокированные вершины внешнего кольца (индексы вершин). Не двигаются при прокрутке по контуру.</summary>
        public readonly HashSet<int> lockedVertices = new HashSet<int>();

        public ArtMeshAsset Asset => renderer != null ? renderer.Asset : null;

        public ArtMeshView View
        {
            get
            {
                var a = Asset;
                return (a != null && viewIndex >= 0 && viewIndex < a.views.Count) ? a.views[viewIndex] : null;
            }
        }

        public Vector3[] Deltas
        {
            get
            {
                var v = View;
                if (v == null) return null;
                return kind == ProxyKind.Lattice ? v.latticeDelta : v.vertexDelta;
            }
        }

        public bool IsAlive
        {
            get
            {
                if (root == null || points == null || renderer == null) return false;
                var d = Deltas;
                return d != null && d.Length == points.Length;
            }
        }
    }
}
