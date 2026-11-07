using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Поза детали (только Transform): позиция относительно точки привязки, поворот, масштаб.
    /// Позиция задана в осях ArtMesh-детали, к которой привязана деталь.
    /// </summary>
    public struct ArtDetailPose
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public ArtDetailPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public static ArtDetailPose Identity => new ArtDetailPose(Vector3.zero, Quaternion.identity, Vector3.one);
    }
}
