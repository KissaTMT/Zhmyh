using UnityEngine;

namespace ArtMesh
{
    /// <summary>Прочитанные из меша данные для CPU-вычислений: базовые позиции и треугольники.</summary>
    public sealed class ArtMeshGeometry
    {
        public readonly Mesh mesh;
        public readonly Vector3[] basePos;
        public readonly int[] triangles;

        public int VertexCount => basePos.Length;
        public bool IsValid => basePos.Length > 0;
        public bool IsFor(Mesh m) => m != null && mesh == m && m.vertexCount == basePos.Length;

        ArtMeshGeometry(Mesh m, Vector3[] b, int[] t) { mesh = m; basePos = b; triangles = t; }

        public static ArtMeshGeometry FromMesh(Mesh m) => new ArtMeshGeometry(m, m.vertices, m.GetTriangles(0));
    }
}
