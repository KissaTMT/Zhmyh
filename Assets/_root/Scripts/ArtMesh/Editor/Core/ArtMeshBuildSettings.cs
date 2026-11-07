using System;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Параметры построения радиального меша. Только редактор; раскладка вершин после построения хранится в ArtMeshTopology.</summary>
    [Serializable]
    public sealed class ArtMeshBuildSettings
    {
        [Range(8, 128)] public int rays = 64;                 // лучей от якоря (вершин на кольце)
        [Range(1, 24)] public int rings = 7;                  // колец от якоря до контура
        [Range(0.01f, 0.99f)] public float alphaThreshold = 0.2f;
        [Range(0.3f, 2f)] public float ringBias = 1f;         // < 1 — кольца гуще у контура
        [Range(0, 4)] public int rimSmooth = 1;               // проходы сглаживания контура
    }
}
