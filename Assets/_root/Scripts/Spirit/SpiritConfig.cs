using System.Collections.Generic;
using UnityEngine;

namespace Spirit
{
    [CreateAssetMenu(fileName = "PresentConfig", menuName = "Configs/PresentConfig")]
    public class SpiritConfig : ScriptableObject
    {
        
        public Vector3 View;

        [Space]

        [Header("Mesh Data")]
        [Space]
        public Vector2[] Uvs;

        [Space]

        [Header("Bone Data")]
        [Space]
        public string[] Names;
        public Vector3[] Positions;
        public Quaternion[] Rotations;
        public Vector3[] Scales;
    }
}
