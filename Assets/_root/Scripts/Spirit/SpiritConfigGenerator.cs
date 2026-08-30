#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spirit
{
    public class SpiritConfigGenerator
    {
        private Transform _root;
        private Rect[] _rects;
        public void SetRoot(Transform root) => _root = root;
        public void SetReference(SpiritConfig config)
        {
            if (config == null) return;

            _rects = config.Rects;
        }

        public SpiritConfig GenerateConfig(Mesh referenceMesh, int gridX, int gridY)
        {
            if (_root == null || referenceMesh == null)
            {
                Debug.LogError("Root or mesh is null!");
                return null;
            }

            // Сортируем рендереры так же, как в MeshBuilder (по убыванию Z)

            var renderers = _root.GetComponentsInChildren<SpriteRenderer>().ToArray();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("No SpriteRenderers found.");
                return null;
            }

            var config = ScriptableObject.CreateInstance<SpiritConfig>();

            // ---- 1. Генерация UV (абсолютные) в ТОЧНОМ ПОРЯДКЕ, как в MeshBuilder ----
            int vertsPerRow = gridX + 1;
            int vertsPerCol = gridY + 1;
            int vertsPerSprite = vertsPerRow * vertsPerCol;

            // Проверяем, что количество вершин в меше соответствует ожидаемому
            int expectedVertexCount = renderers.Length * vertsPerSprite;
            if (referenceMesh.vertexCount != expectedVertexCount)
            {
                Debug.LogWarning($"Vertex count mismatch: mesh has {referenceMesh.vertexCount}, expected {expectedVertexCount}. " +
                                 "Check that gridX/gridY match those used in MeshBuilder.");
                // Продолжаем, но может быть ошибка
            }

            var uvs = new List<Vector2>(expectedVertexCount);
            var rects = new List<Rect>(renderers.Length);

            // Проходим по спрайтам в том же порядке, что и в MeshBuilder
            foreach (var node in renderers)
            {
                var sprite = node.sprite;
                if (sprite == null) continue;

                Rect uvRect = GetSpriteUVRect(sprite);
                bool flipX = node.flipX;
                bool flipY = node.flipY;

                rects.Add(uvRect);

                // Генерируем UV для каждой вершины сетки (row-major, как в MeshBuilder)
                for (int row = 0; row <= gridY; row++)
                {
                    for (int col = 0; col <= gridX; col++)
                    {
                        float tX = (float)col / gridX;
                        float tY = (float)row / gridY;

                        float u = Mathf.Lerp(uvRect.xMin, uvRect.xMax, tX);
                        float v = Mathf.Lerp(uvRect.yMin, uvRect.yMax, tY);

                        // Применяем зеркалирование (если включено)
                        if (flipX) u = uvRect.xMax + uvRect.xMin - u;
                        if (flipY) v = uvRect.yMax + uvRect.yMin - v;

                        uvs.Add(new Vector2(u, v));
                    }
                }

                // Отладка: выводим имена спрайтов и количество добавленных UV
                Debug.Log($"Config: processed sprite '{sprite.name}' -> added {vertsPerSprite} UVs");
            }

            // Проверяем, что количество UV совпадает с количеством вершин в меше
            if (uvs.Count != referenceMesh.vertexCount)
            {
                Debug.LogError($"UV count ({uvs.Count}) does not match vertex count ({referenceMesh.vertexCount})!");
                // Попробуем обрезать или дополнить (но это костыль)
                // В идеале — нужно исправить параметры сетки
            }

            config.Uvs = uvs.ToArray();
            config.Rects = rects.ToArray();

            // ---- 2. Данные о костях из болванки (без изменений) ----
            var boneDataList = new List<BoneData>();
            var boneNameToIndex = new Dictionary<string, int>();

            for (var i=0;i<renderers.Length;i++)
            {
                var node = renderers[i];
                if (node.sprite == null) continue;

                string centerName = $"Bone_{node.name}_Center";

                var scale = node.transform.localScale;

                if(_rects != null)
                {
                    var sr = GetSpriteUVRect(node.sprite);
                    var v = new Vector3(_rects[i].width / sr.width, _rects[i].height / sr.height, 1);
                    scale = new Vector3(scale.x/v.x, scale.y / v.y, scale.z / v.z);
                }

                var centerData = new BoneData
                {
                    name = centerName,
                    parentName = null,
                    localPosition = node.transform.localPosition,
                    localRotation = node.transform.localRotation,
                    localScale = scale
                };
                boneDataList.Add(centerData);
                boneNameToIndex[centerName] = boneDataList.Count - 1;

                //float width = node.sprite.rect.width / node.sprite.pixelsPerUnit;
                //float height = node.sprite.rect.height / node.sprite.pixelsPerUnit;

                //var anchors = new (string suffix, Vector3 offset)[]
                //{
                //    ("Left", new Vector3(-width / 2, 0, 0)),
                //    ("Right", new Vector3(width / 2, 0, 0)),
                //    ("Top", new Vector3(0, height / 2, 0)),
                //    ("Bottom", new Vector3(0, -height / 2, 0))
                //};

                //foreach (var anchor in anchors)
                //{
                //    string anchorName = $"Bone_{node.name}_{anchor.suffix}";
                //    var anchorData = new BoneData
                //    {
                //        name = anchorName,
                //        parentName = centerName,
                //        localPosition = anchor.offset,
                //        localRotation = Quaternion.identity,
                //        localScale = Vector3.one
                //    };
                //    boneDataList.Add(anchorData);
                //    boneNameToIndex[anchorName] = boneDataList.Count - 1;
                //}
            }

            config.Names = boneDataList.Select(b => b.name).ToArray();
            config.Positions = boneDataList.Select(b => b.localPosition).ToArray();
            config.Rotations = boneDataList.Select(b => b.localRotation).ToArray();
            config.Scales = boneDataList.Select(b => b.localScale).ToArray();
            //config.boneParentIndices = boneDataList.Select(b =>
            //{
            //    if (string.IsNullOrEmpty(b.parentName)) return -1;
            //    if (boneNameToIndex.TryGetValue(b.parentName, out int parentIdx))
            //        return parentIdx;
            //    return -1;
            //}).ToList();

            Debug.Log($"Config generated: {config.Uvs.Length} UVs, {config.Names.Length} bones.");
            return config;
        }

        private Rect GetSpriteUVRect(Sprite sprite)
        {
            var tex = sprite.texture;
            var r = sprite.textureRect;
            return new Rect(
                r.x / tex.width,
                r.y / tex.height,
                r.width / tex.width,
                r.height / tex.height
            );
        }

        public string SaveConfigAsset(SpiritConfig config, string assetPath)
        {
            if (config == null) return null;

            var dir = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            if (AssetDatabase.LoadAssetAtPath<SpiritConfig>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return assetPath;
        }

        private class BoneData
        {
            public string name;
            public string parentName;
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
        }
    }
}
#endif