using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Components
{
    public class GroundSphereOverlapChecker : IGroundChecker, IComponent
    {
        private LayerMask _layerMask;
        private Transform _ignore;

        private Collider[] _buffer;
        public GroundSphereOverlapChecker(LayerMask layerMask) : this(layerMask, null, 4) { }
        public GroundSphereOverlapChecker(LayerMask layerMask, Transform ignore) : this(layerMask, ignore, 4) { }
        public GroundSphereOverlapChecker(LayerMask layerMask, int bufferSize) : this(layerMask,null,bufferSize) { }
        public GroundSphereOverlapChecker(LayerMask layerMask, Transform ignore, int bufferSize)
        {
            _layerMask = layerMask;
            _ignore = ignore;
            _buffer = new Collider[bufferSize];
        }
        public bool Check(Vector3 point, float radius)
        { 
            var count = Physics.OverlapSphereNonAlloc(point, radius, _buffer,_layerMask);
            if (_ignore == null) return count > 0;

            for(var i = 0; i < count; i++)
            {
                if (_buffer[i].transform != _ignore) return true;
            }
            return false;
        }
    }
}
