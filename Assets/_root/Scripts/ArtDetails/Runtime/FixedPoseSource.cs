using ArtMesh;

namespace ArtDetails
{
    /// <summary>Ровно одна поза без смешивания (тесты, превью).</summary>
    public sealed class FixedPoseSource : IArtDetailPoseSource
    {
        public ArtDetailPose Pose;

        public FixedPoseSource(ArtDetailPose pose) { Pose = pose; }
        public FixedPoseSource(ArtDetailView view) { Pose = view.ToPose(); }

        public ArtDetailPose Resolve(ArtDetailAsset asset, ArtMeshRenderer target, ArtDetailPoseBlender blender) => Pose;
    }
}
