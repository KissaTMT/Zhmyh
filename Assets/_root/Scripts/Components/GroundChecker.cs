using UnityEngine;

namespace Components
{
    public class GroundChecker : IComponent
    {
        private LayerMask _layerMask;
        public GroundChecker(LayerMask layerMask)
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
