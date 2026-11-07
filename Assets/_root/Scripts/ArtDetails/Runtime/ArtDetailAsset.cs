using System.Collections.Generic;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>Данные детали (ScriptableObject): список видов с Transform. Данных редактора здесь нет.</summary>
    [CreateAssetMenu(menuName = "ArtDetails/Detail Asset", fileName = "NewDetail")]
    public sealed class ArtDetailAsset : ScriptableObject
    {
        [Range(0.5f, 6f)] public float weightPower = 2f;    // 1 ≈ кусочно-линейно, больше = «липнет» к ближайшему виду
        public List<ArtDetailView> views = new List<ArtDetailView>();

        public void EnsureData()
        {
            if (views == null) views = new List<ArtDetailView>();
        }

        void OnValidate() { EnsureData(); }
    }
}
