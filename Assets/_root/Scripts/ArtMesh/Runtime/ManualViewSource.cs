using UnityEngine;

namespace ArtMesh
{
    /// <summary>Направление задано углами (отладка, тесты, кат-сцены): виды смешиваются по нему.</summary>
    public sealed class ManualViewSource : IArtMeshViewSource
    {
        public float Yaw;
        public float Pitch;

        public ManualViewSource(float yaw, float pitch) { Yaw = yaw; Pitch = pitch; }

        public Vector3 Resolve(ArtMeshAsset asset, Transform part, ArtMeshViewBlender blender, ArtMeshState state)
        {
            Vector3 l = ArtMeshViewDirection.FromAngles(Yaw, Pitch);
            blender.Blend(asset, l, state);
            return l;
        }
    }
}
