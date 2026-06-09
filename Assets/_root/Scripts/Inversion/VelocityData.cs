using UnityEngine;

namespace Inversion
{
    public struct VelocityData
    {
        public Vector3 LineirVelocity;
        public Vector3 AngularVelocity;

        public VelocityData(Vector3 velocity, Vector3 angularVelocity)
        {
            LineirVelocity = velocity;
            AngularVelocity = angularVelocity;
        }
    }
}
