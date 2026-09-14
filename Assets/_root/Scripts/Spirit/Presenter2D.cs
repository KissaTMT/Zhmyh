using ModestTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spirit
{
    public class Presenter2D : MonoBehaviour
    {
        [SerializeField] private List<SpiritConfig> _configs;
        [SerializeField] private List<Transform> _bonesList;
        private Dictionary<string, Transform> _bones;
        private SkinnedMeshRenderer _smr;
        private Camera _camera;
        private SpiritConfig _currentConfig;
        private void Awake()
        {
            _smr = GetComponent<SkinnedMeshRenderer>();
            _camera = Camera.main;

            _bones = new Dictionary<string, Transform>();

            for (var i=0; i< _bonesList.Count; i++)
            {
                _bones[_bonesList[i].name] = _bonesList[i];
            }

            ApplyConfig(_configs[0]);
        }
        private void OnDisable()
        {
            ApplyConfig(_configs[0]);
        }

        private void Update()
        {
            _currentConfig = CalculateConfig();
            ApplyConfig(_currentConfig);
        }
        private void ApplyConfig(SpiritConfig config)
        {
            _smr.sharedMesh.uv = config.Uvs;
            foreach (var name in _bones.Keys)
            {
                var data = config.BoneDatas[name];

                _bones[name].localPosition = data.Position;
                _bones[name].localRotation = data.Rotation;
                _bones[name].localScale = data.Scale;
            }
            _smr.sharedMesh.UploadMeshData(false);
        }
        private SpiritConfig CalculateConfig()
        {
            // 1. Направление объекта (родителя) и камеры, спроецированные на плоскость XZ
            Vector3 objForward = transform.parent != null ? transform.parent.forward : transform.forward;
            objForward.y = 0f;
            if (objForward.sqrMagnitude < 0.0001f) return _currentConfig;
            objForward.Normalize();

            Vector3 camForward = _camera.transform.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude < 0.0001f) return _currentConfig;
            camForward.Normalize();

            // 2. Угол между направлениями в плоскости [-180, 180]
            //    0°   — камера смотрит туда же, куда объект (т.е. мы видим спину)
            //    180° — камера смотрит навстречу (мы видим лицо)
            float angle = Vector3.SignedAngle(objForward, camForward, Vector3.up);

            // 3. Нормализуем в [0, 360) для удобства сравнения с config.Angle

            // 4. Мёртвая зона — не пересчитываем, если угол почти не менялся
            if (_currentConfig !=null && !float.IsNaN(_currentConfig.Angle) &&
                Mathf.Abs(Mathf.DeltaAngle(_currentConfig.Angle, angle)) < 2f)
                return _currentConfig;

            // 5. Ищем конфиг с ближайшим углом (DeltaAngle корректно обрабатывает переход 359°→0°)
            SpiritConfig best = null;
            float bestDelta = float.MaxValue;
            for (int i = 0; i < _configs.Count; i++)
            {
                var cfg = _configs[i];
                if (cfg == null) continue;
                float delta = Mathf.Abs(Mathf.DeltaAngle(angle, cfg.Angle));
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = cfg;
                }
            }

            if (best == null || best == _currentConfig) return _currentConfig;

            Debug.Log(best.Angle);

            return best;
        }
    }
}
