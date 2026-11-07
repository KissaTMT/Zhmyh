using ArtMesh;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Деталь (глаз, рот и т.п.): GameObject, привязанный к месту на ArtMesh. Тонкий компонент:
    ///   1. точка привязки на поверхности меша (ArtDetailAnchorResolver);
    ///   2. поза (только Transform) из источника позы, по умолчанию по направлению цели (IArtDetailPoseSource);
    ///   3. маска силуэта (ArtDetailMaskBinder), чтобы деталь не выходила за границы меша.
    ///
    /// Контракт: деталь является ПРЯМЫМ ДОЧЕРНИМ объектом GameObject с ArtMeshRenderer (цель), тогда локальный
    /// Transform детали = пространство ArtMesh. Компонент работает после ArtMeshRenderer (DefaultExecutionOrder).
    /// ArtMesh он только читает и не изменяет.
    ///
    /// Компонент живёт и в редакторе, и в рантайме, и его вызывают снаружи до OnEnable и после перезагрузки скриптов.
    /// Поэтому все внутренние объекты создаются лениво в EnsureInitialized(), а не инициализаторами полей.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)]
    public sealed class ArtDetail : MonoBehaviour
    {
        [SerializeField] ArtMeshRenderer target;
        [SerializeField] ArtDetailAsset asset;
        [SerializeField] ArtDetailBinding binding;
        [SerializeField] ArtDetailMaskSettings mask;

        // не сериализуются, создаются лениво
        ArtDetailPoseBlender blender;
        ArtDetailMaskBinder maskBinder;
        DirectionPoseSource directionSource;
        Renderer[] renderers;
        bool paused;
        Vector3 anchor;
        bool anchorValid;

        public ArtMeshRenderer Target
        {
            get => target;
            set { target = value; EnsureInitialized(); Refresh(); }
        }

        public ArtDetailAsset Asset
        {
            get => asset;
            set { asset = value; EnsureInitialized(); Refresh(); }
        }

        public ArtDetailBinding Binding { get { EnsureInitialized(); return binding; } }
        public ArtDetailMaskSettings Mask { get { EnsureInitialized(); return mask; } }

        /// <summary>Подменяет источник позы. null = по направлению цели. Не сериализуется.</summary>
        public IArtDetailPoseSource PoseSource { get; set; }

        /// <summary>false: компонент не пишет Transform (его двигает кто-то другой: аниматор, редактор). Привязка и маска работают.</summary>
        public bool Driving { get => !paused; set => paused = !value; }

        public bool HasAnchor => anchorValid;
        public Vector3 Anchor => anchor;
        public bool IsChildOfTarget => target != null && transform.parent == target.transform;

        void OnEnable() { EnsureInitialized(); RefreshRenderers(); Refresh(); }
        void LateUpdate() { Refresh(); }
        void OnValidate() { if (isActiveAndEnabled) { EnsureInitialized(); RefreshRenderers(); Refresh(); } }

        void EnsureInitialized()
        {
            if (blender == null) blender = new ArtDetailPoseBlender();
            if (maskBinder == null) maskBinder = new ArtDetailMaskBinder();
            if (directionSource == null) directionSource = new DirectionPoseSource();
            if (binding == null) binding = new ArtDetailBinding();
            if (mask == null) mask = new ArtDetailMaskSettings();
            if (renderers == null) renderers = GetComponentsInChildren<Renderer>(true);
        }

        /// <summary>Пересобирает список рендереров детали (после добавления или удаления дочерних объектов).</summary>
        public void RefreshRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        /// <summary>Пересчитывает привязку, позу и маску.</summary>
        public void Refresh()
        {
            EnsureInitialized();
            if (target == null) { anchorValid = false; return; }

            anchorValid = ArtDetailAnchorResolver.TryResolve(target, binding, out anchor);

            if (anchorValid && !paused && asset != null)
            {
                IArtDetailPoseSource source = PoseSource ?? directionSource;
                ApplyPose(source.Resolve(asset, target, blender));
            }

            int count = SilhouetteCache.Get(target, out Vector2[] points);
            maskBinder.Apply(renderers, points, count, target.transform, mask);
        }

        /// <summary>Применяет позу (относительно точки привязки) к локальному Transform детали.</summary>
        public void ApplyPose(ArtDetailPose pose)
        {
            EnsureInitialized();
            var t = transform;
            t.localPosition = anchor + pose.position;
            t.localRotation = pose.rotation;
            t.localScale = pose.scale;
        }

        /// <summary>Текущая поза детали относительно точки привязки (обратная операция к ApplyPose).</summary>
        public ArtDetailPose ReadPose()
        {
            var t = transform;
            return new ArtDetailPose(t.localPosition - anchor, t.localRotation, t.localScale);
        }
    }
}
