using System.Collections.Generic;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>Данные детали (ScriptableObject): список видов с Transform. Данных редактора здесь нет.</summary>
    [CreateAssetMenu(menuName = "ArtDetails/Detail Asset", fileName = "NewDetail")]
    public sealed class ArtDetailAsset : ScriptableObject
    {
        public ArtDetailBlendMode blendMode = ArtDetailBlendMode.Local;
        [Range(0.5f, 12f)] public float weightPower = 2f;   // только для Shepard: больше = «липнет» к ближайшему виду
        public List<ArtDetailView> views = new List<ArtDetailView>();

        public void EnsureData()
        {
            if (views == null) views = new List<ArtDetailView>();
        }

        void OnValidate() { EnsureData(); }
    }
}
