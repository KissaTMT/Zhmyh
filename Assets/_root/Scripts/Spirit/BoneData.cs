using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spirit
{
    [Serializable]
    public struct BoneData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }
}
