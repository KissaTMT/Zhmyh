using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Источник вида: откуда компонент берёт форму на этот кадр. Это отдельный слой, через который редактор, тесты и
    /// катсцены управляют ArtMeshRenderer, не требуя от компонента специальных «редакторских» режимов.
    /// По умолчанию компонент использует CameraViewSource.
    /// </summary>
    public interface IArtMeshViewSource
    {
        /// <summary>Заполняет state и возвращает направление «деталь → камера» (локальные оси), чтобы показать yaw/pitch.</summary>
        Vector3 Resolve(ArtMeshAsset asset, Transform part, ArtMeshViewBlender blender, ArtMeshState state);
    }
}
