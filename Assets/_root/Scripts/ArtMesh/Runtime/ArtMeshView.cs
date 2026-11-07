using System;
using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Вид арт-меша (данные): направление камеры, трансформ вида, смещения сетки и вершин.
    /// Направление — полная сфера: yaw 0…360 (0 = камера на +Z детали, 90 = на +X, 180 = сзади, 270 = на −X).
    /// Каждый вид настраивается вручную: левой и правой стороны как отдельных понятий нет.
    /// Смещения трёхмерные: Z = глубина (+Z смотрит в сторону «переда» детали).
    /// </summary>
    [Serializable]
    public sealed class ArtMeshView
    {
        public string name = "view";
        [Range(0, 360)] public float yaw;
        [Range(-90, 90)] public float pitch;                 // > 0: камера выше
        public Vector3 scale = Vector3.one;                  // как Scale в GameObject
        public Vector3 offset = Vector3.zero;                // как Position; offset.z = глубина вида
        [HideInInspector] public Vector3[] latticeDelta;     // смещения узлов сетки, cols*rows
        [HideInInspector] public Vector3[] vertexDelta;      // финальные правки вершин (после сетки), null = нет

        public Vector3 Direction() => ArtMeshViewDirection.FromAngles(yaw, pitch);

        public ArtMeshView Clone()
        {
            var c = (ArtMeshView)MemberwiseClone();
            c.latticeDelta = latticeDelta != null ? (Vector3[])latticeDelta.Clone() : null;
            c.vertexDelta = vertexDelta != null ? (Vector3[])vertexDelta.Clone() : null;
            return c;
        }
    }
}
