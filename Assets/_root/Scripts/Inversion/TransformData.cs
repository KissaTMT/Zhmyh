using UnityEngine;

namespace Inversion
{
    public struct TransformData
    {
        public Vector3 Position;
        public Vector3 Rotation;

        public TransformData(Vector3 position, Vector3 rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
