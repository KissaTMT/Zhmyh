using ArtMesh;

namespace ArtDetails
{
    /// <summary>
    /// Источник позы: откуда деталь берёт Transform на этот кадр. Отдельный слой для тестов, кат-сцен и редактора.
    /// По умолчанию используется DirectionPoseSource (по направлению камеры цели).
    /// </summary>
    public interface IArtDetailPoseSource
    {
        ArtDetailPose Resolve(ArtDetailAsset asset, ArtMeshRenderer target, ArtDetailPoseBlender blender);
    }
}
