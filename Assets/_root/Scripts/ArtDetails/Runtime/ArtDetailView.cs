using System;
using ArtMesh;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Вид детали (данные): направление камеры и ТОЛЬКО данные Transform.
    /// Направление задаётся так же, как у видов ArtMesh: yaw 0…360, pitch −90…90.
    /// Позиция хранится относительно точки привязки на ArtMesh, поэтому деталь едет вместе с деформацией меша.
    /// </summary>
    [Serializable]
    public sealed class ArtDetailView
    {
        public string name = "view";
        [Range(0, 360)] public float yaw;
        [Range(-90, 90)] public float pitch;
        public Vector3 position = Vector3.zero;     // относительно точки привязки, в осях ArtMesh-детали
        public Vector3 euler = Vector3.zero;        // поворот в градусах
        public Vector3 scale = Vector3.one;

        public Vector3 Direction() => ArtMeshViewDirection.FromAngles(yaw, pitch);

        public ArtDetailPose ToPose() => new ArtDetailPose(position, Quaternion.Euler(euler), scale);

        public void Set(Vector3 position, Vector3 euler, Vector3 scale)
        {
            this.position = position;
            this.euler = euler;
            this.scale = scale;
        }
    }
}
