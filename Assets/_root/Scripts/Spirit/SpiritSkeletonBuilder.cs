#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Spirit
{
    public class SpiritSkeletonBuilder
    {
        private Transform _root;
        private GameObject _skinnedRoot;

        public void SetRoot(Transform root) => _root = root;

        public GameObject BuildSkeleton()
        {
            if (_root == null)
            {
                Debug.LogError("Root is null!");
                return null;
            }

            // Удаляем старый скелет
            ClearSkeleton();

            // Создаём новый корень скелета
            _skinnedRoot = new GameObject(_root.name + "_SkinnedRoot");
            _skinnedRoot.transform.SetParent(_root.parent, false);
            _skinnedRoot.transform.position = _root.position;
            _skinnedRoot.transform.rotation = _root.rotation;
            _skinnedRoot.transform.localScale = _root.localScale;

            // Получаем все SpriteRenderer'ы
            SpriteRenderer[] renderers = _root.GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning("No SpriteRenderers found.");
                return _skinnedRoot;
            }

            // Создаём кости для каждого спрайта
            foreach (var node in renderers)
            {
                if (node.sprite == null) continue;

                // Центральная кость
                GameObject centerBone = new GameObject($"Bone_{node.name}_Center");
                centerBone.transform.SetParent(_skinnedRoot.transform, false);
                centerBone.transform.localPosition = node.transform.localPosition;
                centerBone.transform.localRotation = node.transform.localRotation;

                // Якорные кости
                float width = node.sprite.rect.width / node.sprite.pixelsPerUnit;
                float height = node.sprite.rect.height / node.sprite.pixelsPerUnit;

                CreateAnchorBone(centerBone.transform, "Left", new Vector3(-width / 2, 0, 0));
                CreateAnchorBone(centerBone.transform, "Right", new Vector3(width / 2, 0, 0));
                CreateAnchorBone(centerBone.transform, "Top", new Vector3(0, height / 2, 0));
                CreateAnchorBone(centerBone.transform, "Bottom", new Vector3(0, -height / 2, 0));
            }

            Debug.Log($"Skeleton created with {renderers.Length} center bones.");
            return _skinnedRoot;
        }

        private void CreateAnchorBone(Transform parent, string suffix, Vector3 offset)
        {
            GameObject bone = new GameObject($"{parent.name.Replace("_Center", "")}_{suffix}");
            bone.transform.SetParent(parent, false);
            bone.transform.localPosition = offset;
            bone.transform.localRotation = Quaternion.identity;
        }

        private void ClearSkeleton()
        {
            if (_skinnedRoot != null)
            {
                Object.DestroyImmediate(_skinnedRoot);
                _skinnedRoot = null;
            }
        }
    }
}
#endif