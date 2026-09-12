using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Spirit
{
    [CreateAssetMenu(fileName = "PresentConfig", menuName = "Configs/PresentConfig")]
    public class SpiritConfig : ScriptableObject
    {
        
        public Vector3 Direction;
        public float Angle;

        [HideInInspector] public Vector2[] Uvs;
        [HideInInspector] public Rect[] Rects;


        public Dictionary<string, BoneData> BoneDatas
        {
            get
            {
                if (_boneDatas == null || _boneDatas.Count == 0) KeyValuePair<string, BoneData>.KVPList2Map(_boneDatas, _kvpBoneDatas);
                return _boneDatas;
            }
            set
            {
                _boneDatas = value;
                KeyValuePair<string, BoneData>.Map2KVPList(_boneDatas, _kvpBoneDatas);
            }
        }
        public Dictionary<string, RendererData> ViewDatas
        {
            get
            {
                if (_viewDatas == null || _viewDatas.Count == 0) KeyValuePair<string, RendererData>.KVPList2Map(_viewDatas, _kvpViewDatas);
                
                return _viewDatas;
            }
            set
            {
                _viewDatas = value;

                RecalculateRects();
                RecalculateUvs();

                KeyValuePair<string, RendererData>.Map2KVPList(_viewDatas, _kvpViewDatas);
            }
        }

        private Dictionary<string, RendererData> _viewDatas = new();
        private Dictionary<string, BoneData> _boneDatas = new();

        [SerializeField] private List<KeyValuePair<string, RendererData>> _kvpViewDatas = new();
        [SerializeField] private List<KeyValuePair<string, BoneData>> _kvpBoneDatas = new();

        [SerializeField, HideInInspector] private int _hash;

        private void OnValidate()
        {
            var hash = GetHashCode();

            if(_hash == hash) return;

            RecalculateRects();
            RecalculateUvs();

            KeyValuePair<string, BoneData>.KVPList2Map(_boneDatas, _kvpBoneDatas);
            KeyValuePair<string, RendererData>.KVPList2Map(_viewDatas, _kvpViewDatas);

            _hash = hash;
        }
        public void RecalculateUvs()
        {
            if (_kvpViewDatas == null || _kvpViewDatas.Count == 0) return;
            var uvs = new List<Vector2>();
            foreach (var kvp in _kvpViewDatas)
            {
                uvs.AddRange(ViewUvCalculator.CalculateUvsByView(kvp.Value));
            }
            Uvs = uvs.ToArray();
        }
        public void RecalculateRects()
        {
            if (_kvpViewDatas == null || _kvpViewDatas.Count == 0) return;
            var rects = new List<Rect>();
            foreach (var kvp in _kvpViewDatas)
            {
                rects.Add(ViewUvCalculator.CalculateSpriteRect(kvp.Value.Sprite));
            }
            Rects = rects.ToArray();
        }
        public override int GetHashCode()
        {
            if (_kvpViewDatas == null || _kvpViewDatas.Count == 0) return -1;

            var hash = 0;

            foreach(var kvp in _kvpViewDatas)
            {
                hash += kvp.Value.GetHashCode();
            }

            return hash;
        }
    }
}
