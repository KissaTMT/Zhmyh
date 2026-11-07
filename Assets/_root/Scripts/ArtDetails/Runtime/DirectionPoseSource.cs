using ArtMesh;

namespace ArtDetails
{
    /// <summary>Игровой режим: поза по направлению цели (yaw/pitch ArtMeshRenderer), виды смешиваются.</summary>
    public sealed class DirectionPoseSource : IArtDetailPoseSource
    {
        public ArtDetailPose Resolve(ArtDetailAsset asset, ArtMeshRenderer target, ArtDetailPoseBlender blender)
            => blender.Blend(asset, target.Yaw, target.Pitch);
    }
}
