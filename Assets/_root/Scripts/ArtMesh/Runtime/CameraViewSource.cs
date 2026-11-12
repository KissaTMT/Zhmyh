using UnityEngine;

namespace ArtMesh
{
    /// <summary>Игровой режим: направление берётся от камеры, виды смешиваются по нему.</summary>
    public sealed class CameraViewSource : IArtMeshViewSource
    {
        public Camera Camera;          // null = Camera.main
        public Transform Space;        // оси, в которых считается направление; null = сама деталь
        Vector3 last = Vector3.forward;

        public Vector3 Resolve(ArtMeshAsset asset, Transform part, ArtMeshViewBlender blender, ArtMeshState state)
        {
            Camera cam = Camera != null ? Camera : Camera.main;
            if (cam != null) last = ArtMeshViewDirection.FromCamera(cam, part, Space);
            last.y = 0f;

            if (last.sqrMagnitude > 1e-12f)
                last.Normalize();
            else
                last = Vector3.forward;
            blender.Blend(asset, last, state);
            return last;
        }
    }
}
