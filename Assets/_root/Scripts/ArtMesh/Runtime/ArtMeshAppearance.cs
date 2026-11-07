using System;
using UnityEngine;

namespace ArtMesh
{
    /// <summary>Внешний вид детали: заливка цветом и обводка. Текстур нет.</summary>
    [Serializable]
    public sealed class ArtMeshAppearance
    {
        public Color fillColor = Color.white;
        public bool outline = true;
        public Color outlineColor = Color.black;
        [Min(0f)] public float outlineWidth = 0.03f;   // в локальных единицах меша
    }
}
