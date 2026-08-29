#if UNITY_EDITOR
using Spirit;
using System.Linq;
using UnityEngine;

namespace Spirit
{
    public class SpiritSMRBuilder
    {
        private Transform _root;
        private GameObject _skinnedRoot;
        private Mesh _mesh;
        private SpiritConfig _config;

        public void SetRoot(Transform root) => _root = root;
        public void SetMesh(Mesh mesh) => _mesh = mesh;
        public void SetConfig(SpiritConfig config) => _config = config;
        public void SetSkeleton(GameObject skinnedRoot) => _skinnedRoot = skinnedRoot;

        public GameObject BuildSMR()
        {
            if (_root == null || _mesh == null || _skinnedRoot == null)
            {
                Debug.LogError("Root, Mesh or SkinnedRoot is null!");
                return null;
            }

            // Собираем все кости из _skinnedRoot
            Transform[] bones = _skinnedRoot.GetComponentsInChildren<Transform>().Skip(1).ToArray();

            if (bones.Length == 0)
            {
                Debug.LogError("No bones found in SkinnedRoot!");
                return null;
            }

            // Применяем трансформации костей из конфига (если есть)
            if (_config != null && _config.Names.Length > 0)
            {
                Transform skeletonRoot = _skinnedRoot.transform;
                for (int i = 0; i < _config.Names.Length; i++)
                {
                    string boneName = _config.Names[i];
                    Transform bone = skeletonRoot.Find(boneName);
                    if (bone != null)
                    {
                        bone.localPosition = _config.Positions[i];
                        bone.localRotation = _config.Rotations[i];
                        bone.localScale = _config.Scales[i];
                    }
                    else
                    {
                        Debug.LogWarning($"Bone {boneName} not found in skeleton, skipping.");
                    }
                }
            }

            // Проверяем соответствие bindPoses
            if (bones.Length != _mesh.bindposes.Length)
            {
                Debug.LogError($"Bone count mismatch: mesh has {_mesh.bindposes.Length} bindposes, but {bones.Length} bones found.");
                return null;
            }

            // ---- ПРИМЕНЯЕМ UV ИЗ КОНФИГА ----
            if (_config != null && _config.Uvs != null && _config.Uvs.Length == _mesh.vertexCount)
            {
                _mesh.uv = _config.Uvs;
                _mesh.UploadMeshData(false);
            }
            else
            {
                Debug.LogWarning("Config UVs are missing or don't match vertex count. Using mesh default UVs.");
            }

            // Создаём SMR
            var smr = _skinnedRoot.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) smr = _skinnedRoot.AddComponent<SkinnedMeshRenderer>();
            else smr.sharedMesh = null;

            smr.sharedMesh = _mesh;
            smr.bones = bones;

            // Материал
            var mat = new Material(Shader.Find("Sprites/Default"));
            var firstSprite = _root.GetComponentInChildren<SpriteRenderer>()?.sprite;
            if (firstSprite != null) mat.mainTexture = firstSprite.texture;
            smr.sharedMaterial = mat;

            return _skinnedRoot;
        }
    }
}
#endif