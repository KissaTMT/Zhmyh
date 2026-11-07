using UnityEngine;

namespace ArtMesh
{
    /// <summary>Показывает ровно один вид без смешивания (превью, редактирование, тесты).</summary>
    public sealed class FixedViewSource : IArtMeshViewSource
    {
        public ArtMeshView View;

        public FixedViewSource(ArtMeshView view) { View = view; }

        public Vector3 Resolve(ArtMeshAsset asset, Transform part, ArtMeshViewBlender blender, ArtMeshState state)
        {
            blender.FromView(View, state);
            return View.Direction();
        }
    }
}
