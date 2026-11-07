using System;
using UnityEngine;

namespace ArtMesh
{
    /// <summary>Текущее вычисленное состояние формы (результат смешивания видов). Только данные.</summary>
    public sealed class ArtMeshState
    {
        public Vector3 scale = Vector3.one;
        public Vector3 offset = Vector3.zero;
        public Vector3[] lattice = new Vector3[0];
        public Vector3[] vertexDelta = new Vector3[0];
        public bool hasVertexDelta;

        public void Resize(int latticeCount, int vertexCount)
        {
            if (lattice.Length != latticeCount) lattice = new Vector3[latticeCount];
            if (vertexDelta.Length != vertexCount) vertexDelta = new Vector3[vertexCount];
        }

        public void Clear()
        {
            scale = Vector3.one; offset = Vector3.zero; hasVertexDelta = false;
            Array.Clear(lattice, 0, lattice.Length);
            Array.Clear(vertexDelta, 0, vertexDelta.Length);
        }
    }
}
