#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Spirit
{
    public class SpiritMeshBuilder
    {
        public List<SpriteRenderer> Renderers => _renderers;
        private Transform _root;
        private GameObject _skinnedRoot;
        private List<Transform> _lastBones;
        private List<SpriteRenderer> _renderers;

        public void SetRoot(Transform root) => _root = root;
        public void SetSkinned(GameObject skinnedRoot) => _skinnedRoot = skinnedRoot;

        public Mesh BuildGridMesh(int gridX = 4, int gridY = 4, bool preserveRectSize = true)
        {
            if (_root == null)
            {
                Debug.LogError("Root is null!");
                return null;
            }

            // Проверяем, что скелет создан

            gridX = Mathf.Max(2, gridX);
            gridY = Mathf.Max(2, gridY);

            var renderers = _root.GetComponentsInChildren<SpriteRenderer>();
                //.OrderByDescending(i => i.transform.localPosition.z).ToArray();

            _renderers = renderers.ToList();

            if (renderers.Length == 0)
            {
                Debug.LogWarning("No SpriteRenderers found.");
                return null;
            }

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var boneWeights = new List<BoneWeight>();
            var allBonesList = new List<Transform>();

            foreach (var node in renderers)
            {
                var sprite = node.sprite;
                if (sprite == null) continue;

                float width = sprite.rect.width / sprite.pixelsPerUnit;
                float height = sprite.rect.height / sprite.pixelsPerUnit;

                bool flipX = node.flipX;
                bool flipY = node.flipY;

                // ---- Находим существующие кости (должны быть созданы) ----
                string centerName = $"Bone_{node.name}_Center";
                Transform centerBone = _skinnedRoot.transform.Find(centerName);
                if (centerBone == null)
                {
                    Debug.LogError($"Center bone {centerName} not found. Create skeleton first!");
                    return null;
                }

                int centerIndex = allBonesList.Count;
                allBonesList.Add(centerBone);

                // Якорные кости
                Transform leftBone = centerBone.Find($"Bone_{node.name}_Left");
                Transform rightBone = centerBone.Find($"Bone_{node.name}_Right");
                Transform topBone = centerBone.Find($"Bone_{node.name}_Top");
                Transform bottomBone = centerBone.Find($"Bone_{node.name}_Bottom");

                if (leftBone == null || rightBone == null || topBone == null || bottomBone == null)
                {
                    Debug.LogError($"Anchor bones for {node.name} not found. Create skeleton first!");
                    return null;
                }

                int leftIndex = allBonesList.Count;
                int rightIndex = allBonesList.Count + 1;
                int topIndex = allBonesList.Count + 2;
                int bottomIndex = allBonesList.Count + 3;
                allBonesList.AddRange(new[] { leftBone, rightBone, topBone, bottomBone });

                // ---- Генерация сетки ----
                int verticesPerRow = gridX + 1;
                int baseIndex = vertices.Count;

                for (int row = 0; row <= gridY; row++)
                {
                    for (int col = 0; col <= gridX; col++)
                    {
                        float tX = (float)col / gridX;
                        float tY = (float)row / gridY;

                        float localX = (tX - 0.5f) * width;
                        float localY = (tY - 0.5f) * height;
                        var localPos = new Vector3(localX, localY, 0);

                        var transformed = node.transform.localPosition +
                                          node.transform.localRotation * Vector3.Scale(localPos, node.transform.localScale);
                        vertices.Add(transformed);

                        uvs.Add(new Vector2(tX, tY));

                        // ---- Веса ----
                        float normX = (tX - 0.5f) * 2f;
                        float normY = (tY - 0.5f) * 2f;

                        float wCenter = 1f, wLeft = 0f, wRight = 0f, wTop = 0f, wBottom = 0f;

                        if (normX < 0) wLeft = Mathf.Clamp01(Mathf.Abs(normX) * 0.5f);
                        if (normX > 0) wRight = Mathf.Clamp01(Mathf.Abs(normX) * 0.5f);
                        if (normY > 0) wTop = Mathf.Clamp01(Mathf.Abs(normY) * 0.5f);
                        if (normY < 0) wBottom = Mathf.Clamp01(Mathf.Abs(normY) * 0.5f);

                        float childSum = wLeft + wRight + wTop + wBottom;
                        wCenter = Mathf.Clamp01(1f - childSum);

                        float total = wCenter + childSum;
                        if (total > 0.001f)
                        {
                            wCenter /= total;
                            wLeft /= total;
                            wRight /= total;
                            wTop /= total;
                            wBottom /= total;
                        }
                        else wCenter = 1f;

                        var weights = new (float, int)[]
                        {
                            (wCenter, centerIndex),
                            (wLeft, leftIndex),
                            (wRight, rightIndex),
                            (wTop, topIndex),
                            (wBottom, bottomIndex)
                        };
                        System.Array.Sort(weights, (a, b) => b.Item1.CompareTo(a.Item1));

                        var bw = new BoneWeight
                        {
                            boneIndex0 = weights[0].Item2,
                            weight0 = weights[0].Item1,
                            boneIndex1 = weights[1].Item2,
                            weight1 = weights[1].Item1,
                            boneIndex2 = weights[2].Item2,
                            weight2 = weights[2].Item1,
                            boneIndex3 = weights[3].Item2,
                            weight3 = weights[3].Item1
                        };
                        boneWeights.Add(bw);
                    }
                }

                // ---- Треугольники ----
                for (int row = 0; row < gridY; row++)
                {
                    for (int col = 0; col < gridX; col++)
                    {
                        int topLeft = baseIndex + row * verticesPerRow + col;
                        int topRight = topLeft + 1;
                        int bottomLeft = topLeft + verticesPerRow;
                        int bottomRight = bottomLeft + 1;

                        triangles.Add(topLeft);
                        triangles.Add(bottomLeft);
                        triangles.Add(topRight);
                        triangles.Add(topRight);
                        triangles.Add(bottomLeft);
                        triangles.Add(bottomRight);
                    }
                }
            }

            // ---- Создание меша ----
            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.boneWeights = boneWeights.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var bindPoses = new Matrix4x4[allBonesList.Count];
            for (int i = 0; i < allBonesList.Count; i++)
                bindPoses[i] = allBonesList[i].worldToLocalMatrix * _skinnedRoot.transform.localToWorldMatrix;
            mesh.bindposes = bindPoses;

            _lastBones = allBonesList;
            Debug.Log($"Mesh built: {vertices.Count} verts, {triangles.Count / 3} tris, {allBonesList.Count} bones.");

            return mesh;
        }

        public string SaveMeshAsset(Mesh mesh, string assetPath)
        {
            if (mesh == null) return null;

            var dir = System.IO.Path.GetDirectoryName(assetPath);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null)
                AssetDatabase.DeleteAsset(assetPath);

            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return assetPath;
        }
    }
}
#endif