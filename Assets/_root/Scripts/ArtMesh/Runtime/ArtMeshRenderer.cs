using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Тонкий компонент: читает ArtMeshAsset, получает форму от источника вида (по умолчанию камера) и отдаёт её
    /// в ArtMeshRenderBinder. В нём нет ни функций редактора, ни понятия зеркала, ни билбординга.
    /// Деталь лежит в плоскости XY своего Transform; «перед» = ось +Z.
    ///
    /// Редактор, тесты и кат-сцены подменяют источник через ViewSource (слой IArtMeshViewSource).
    ///
    /// Компонент живёт и в редакторе (ExecuteAlways), и в рантайме, и его вызывают снаружи до OnEnable и после
    /// перезагрузки скриптов. Поэтому ВСЕ внутренние объекты создаются лениво в EnsureInitialized(), а не инициализаторами
    /// полей, и каждая точка входа вызывает его первой.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class ArtMeshRenderer : MonoBehaviour
    {
        [SerializeField] ArtMeshAsset asset;
        [SerializeField] Material material;                 // ArtMesh/FlatDeform
        [SerializeField] ArtMeshAppearance appearance;

        [Header("Направление взгляда (источник по умолчанию)")]
        [SerializeField] Camera viewCamera;                 // по умолчанию Camera.main
        [SerializeField] Transform viewSpace;               // оси, в которых считается направление; по умолчанию сама деталь

        // не сериализуются, создаются лениво
        ArtMeshState state;
        ArtMeshViewBlender blender;
        ArtMeshRenderBinder binder;
        CameraViewSource cameraSource;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        public ArtMeshAsset Asset
        {
            get => asset;
            set { asset = value; EnsureInitialized(); Rebind(); Refresh(); }
        }

        public Material Material => material;

        public ArtMeshAppearance Appearance { get { EnsureInitialized(); return appearance; } }
        public ArtMeshState State { get { EnsureInitialized(); return state; } }

        /// <summary>Направление камеры в углах: yaw 0…360, pitch −90…90.</summary>
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        /// <summary>Подменяет источник вида. null = камера по умолчанию. Не сериализуется.</summary>
        public IArtMeshViewSource ViewSource { get; set; }

        void OnEnable() { EnsureInitialized(); Rebind(); Refresh(); }
        void OnDisable() { if (binder != null) binder.Dispose(); }
        void LateUpdate() { Refresh(); }
        void OnValidate() { if (isActiveAndEnabled) { EnsureInitialized(); Rebind(); Refresh(); } }

        /// <summary>Создаёт всё, что компонент использует. Безопасно вызывать сколько угодно раз и из любого состояния.</summary>
        void EnsureInitialized()
        {
            if (state == null) state = new ArtMeshState();
            if (blender == null) blender = new ArtMeshViewBlender();
            if (binder == null) binder = new ArtMeshRenderBinder();
            if (cameraSource == null) cameraSource = new CameraViewSource();
            if (appearance == null) appearance = new ArtMeshAppearance();
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>Переназначает меш и материалы (после смены ассета, материала или перепекания меша).</summary>
        public void Rebind()
        {
            EnsureInitialized();
            if (meshFilter == null || meshRenderer == null) return;
            if (asset == null) { meshFilter.sharedMesh = null; return; }
            asset.EnsureViewData();
            meshFilter.sharedMesh = asset.mesh;
            binder.BindMaterials(meshRenderer, asset.mesh, material);
        }

        /// <summary>Пересчитывает состояние и отправляет его в шейдер.</summary>
        public void Refresh()
        {
            EnsureInitialized();
            if (asset == null || asset.mesh == null || material == null) return;
            if (meshFilter == null || meshRenderer == null) return;
            if (meshFilter.sharedMesh != asset.mesh) Rebind();

            state.Resize(asset.LatticeCount, asset.VertexCount);

            IArtMeshViewSource source = ViewSource;
            if (source == null)
            {
                cameraSource.Camera = viewCamera;
                cameraSource.Space = viewSpace;
                source = cameraSource;
            }
            Vector3 l = source.Resolve(asset, transform, blender, state);

            float yaw, pitch;
            ArtMeshViewDirection.ToAngles(l, out yaw, out pitch);
            Yaw = yaw; 
            Pitch = pitch;

            binder.Apply(meshRenderer, asset, state, appearance);
        }
    }
}
