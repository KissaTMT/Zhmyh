using UnityEngine;

namespace Components
{
    public class GroundSphereChecker : IGroundChecker
    {
        private LayerMask _layerMask;
        public GroundSphereChecker(LayerMask layerMask)
        {
            _layerMask = layerMask;
        }
        public bool Check(Vector3 point, float radius = 0.1f)
        {
            var result = Physics.CheckSphere(point, radius, _layerMask);

            return result;
        }
    }
}
