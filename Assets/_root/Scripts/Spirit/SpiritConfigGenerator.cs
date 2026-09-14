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
        private Vector2 _grid;
        public void SetRoot(Transform root) => _root = root;
        public void SetReference(SpiritConfig config)
        {
            if (config == null) return;

            _rects = config.Rects;
        }
        public void SetGrid(float gridX, float gridY)
        {
            _grid = new Vector2(gridX, gridY);
        }

        public SpiritConfig GenerateConfig(Vector3 direction)
        {

            var renderers = _root.GetComponentsInChildren<SpriteRenderer>().ToArray();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("No SpriteRenderers found.");
                return null;
            }

            var config = ScriptableObject.CreateInstance<SpiritConfig>();

            config.Direction = direction;

            
            var boneDatas = new Dictionary<string,BoneData>();
            var viewDatas = new Dictionary<string, RendererData>();

            for (var i=0;i<renderers.Length;i++)
            {
                var node = renderers[i];
                if (node.sprite == null) continue;

                string centerName = $"Bone_{node.name}_Center";

                var scale = node.transform.localScale;

                if(_rects != null)
                {
                    var sr = ViewUvCalculator.CalculateSpriteRect(node.sprite);
                    var v = new Vector3(_rects[i].width / sr.width, _rects[i].height / sr.height, 1);
                    scale = new Vector3(scale.x/v.x, scale.y / v.y, scale.z / v.z);
                }

                var centerData = new BoneData
                {
                    Position = node.transform.localPosition,
                    Rotation = node.transform.localRotation,
                    Scale = scale
                };
                var viewData = new RendererData
                {
                    Sprite = node.sprite,
                    FlipX = node.flipX,
                    FlipY = node.flipY,
                    GridSize = _grid
                };
                boneDatas.Add(centerName,centerData);
                viewDatas.Add(centerName, viewData);

                //boneNameToIndex[centerName] = boneDatas.Count - 1;

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

            config.BoneDatas = boneDatas;
            config.ViewDatas = viewDatas;
            
            return config;
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
    }
}
#endif