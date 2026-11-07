using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Перевод между пространством формы (точки до трансформа вида) и пространством детали (после scale/offset).</summary>
    public static class ViewTransform
    {
        public static Vector2 ToDisplay(ArtMeshView v, Vector3 shape)
            => new Vector2(shape.x * v.scale.x + v.offset.x, shape.y * v.scale.y + v.offset.y);

        public static Vector3 ToShapeDelta(ArtMeshView v, Vector2 displayDelta)
            => new Vector3(Mathf.Abs(v.scale.x) > 1e-4f ? displayDelta.x / v.scale.x : 0f,
                           Mathf.Abs(v.scale.y) > 1e-4f ? displayDelta.y / v.scale.y : 0f, 0f);
    }
}
