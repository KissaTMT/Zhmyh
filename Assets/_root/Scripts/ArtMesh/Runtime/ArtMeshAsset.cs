using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Данные арт-меша (ScriptableObject): запечённый меш, раскладка вершин, конфиг сетки и все виды.
    /// Рантайм-компонент только читает этот ассет. Меш хранится как под-ассет этого же файла.
    /// Никаких данных редактора здесь нет: они лежат в ArtMeshAuthoring (сборка редактора).
    /// </summary>
    [CreateAssetMenu(menuName = "ArtMesh/Art Mesh Asset", fileName = "NewArtMesh")]
    public sealed class ArtMeshAsset : ScriptableObject
    {
        [Header("Меш (создаётся в окне ArtMesh Editor → Mesh → Bake)")]
        public Mesh mesh;
        public ArtMeshTopology topology = new ArtMeshTopology();

        [Header("Сетка деформации и смешивание видов")]
        public bool useLattice = true;
        [Range(2, LatticeMath.MaxDim)] public int latticeCols = 5;
        [Range(2, LatticeMath.MaxDim)] public int latticeRows = 5;
        [Range(0.5f, 6f)] public float weightPower = 2f;      // 1 ≈ кусочно-линейно, больше = «липнет» к ближайшему виду
        [HideInInspector] public Vector2 latticeMin, latticeSize;   // область сетки, задаётся при Bake

        [Header("Виды")]
        public List<ArtMeshView> views = new List<ArtMeshView>();

        [SerializeField, HideInInspector] int dataCols, dataRows;

        [NonSerialized] ArtMeshGeometry geometry;

        public int VertexCount => mesh != null ? mesh.vertexCount : 0;
        public int LatticeCount => latticeCols * latticeRows;

        /// <summary>Кэш геометрии для CPU-вычислений. Рантайму на GPU-пути не нужен.</summary>
        public ArtMeshGeometry Geometry
        {
            get
            {
                if (mesh == null) return null;
                if (geometry == null || !geometry.IsFor(mesh)) geometry = ArtMeshGeometry.FromMesh(mesh);
                return geometry;
            }
        }

        public void InvalidateGeometry() { geometry = null; }

        /// <summary>
        /// Приводит данные видов к текущему разрешению сетки и топологии меша
        /// (передискретизирует смещения сетки, сбрасывает правки вершин при смене топологии). Возвращает true, если что-то изменилось.
        /// </summary>
        public bool EnsureViewData()
        {
            if (views == null) views = new List<ArtMeshView>();
            if (topology == null) topology = new ArtMeshTopology();
            bool changed = false;
            int n = LatticeCount, vc = VertexCount;
            foreach (var v in views)
            {
                if (v.vertexDelta != null && vc > 0 && v.vertexDelta.Length != vc)
                {
                    Debug.LogWarning($"ArtMesh: правки вершин вида '{v.name}' сброшены: топология меша изменилась.", this);
                    v.vertexDelta = null;
                    changed = true;
                }
                if (v.latticeDelta != null && v.latticeDelta.Length == n) continue;
                bool canResample = v.latticeDelta != null && dataCols > 1 && dataRows > 1
                                   && v.latticeDelta.Length == dataCols * dataRows;
                v.latticeDelta = canResample
                    ? LatticeMath.Resample(v.latticeDelta, dataCols, dataRows, latticeCols, latticeRows)
                    : new Vector3[n];
                changed = true;
            }
            if (dataCols != latticeCols || dataRows != latticeRows)
            {
                dataCols = latticeCols; dataRows = latticeRows;
                changed = true;
            }
            return changed;
        }

        void OnValidate() { EnsureViewData(); }
    }
}
