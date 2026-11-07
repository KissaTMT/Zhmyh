using System;

namespace ArtMesh
{
    /// <summary>
    /// Описание раскладки вершин радиального меша (записывается при построении). Нужно инструментам, чтобы понимать,
    /// где центр, кольца и лучи: вершина 0 = центр, дальше rings колец по rays вершин.
    /// </summary>
    [Serializable]
    public sealed class ArtMeshTopology
    {
        public int rays;
        public int rings;
        public float ringBias = 1f;      // кольца лежат на радиусах (r / rings)^ringBias

        public bool IsRadialFor(int vertexCount) => rays > 0 && rings > 0 && vertexCount == 1 + rays * rings;
    }
}
