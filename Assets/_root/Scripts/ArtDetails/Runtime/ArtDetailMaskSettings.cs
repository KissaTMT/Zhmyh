using System;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>Настройки маски: деталь не выходит за силуэт меша, на котором она стоит.</summary>
    [Serializable]
    public sealed class ArtDetailMaskSettings
    {
        public bool enabled = true;
        [Min(0f)] public float inset = 0f;       // насколько отступать внутрь от границы силуэта (локальные единицы)
    }
}
