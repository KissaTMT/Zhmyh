using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace MADCUP.STM
{
    [ExecuteAlways]
    public class SpriteMesh : MonoBehaviour
    {
        public Sprite sprite;
        public Color color = Color.white;

        private Mesh mesh;
        private Material material;

        private const string defaultSpritePath = "Assets/STM (Sprite To Mesh)/Icon_Pack/Square.png";
        private Sprite previousSprite;

        [MenuItem("GameObject/2D Object/SpriteMesh", false, 10)]
        static void CreateSpriteMeshObject(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("SpriteMesh");
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            go.AddComponent<SpriteMesh>();

            // Set position in front of the camera
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            if (sceneCamera != null)
            {
                go.transform.position = sceneCamera.transform.position + sceneCamera.transform.forward * 1.5f;
                go.transform.rotation = Quaternion.identity;
            }

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Selection.activeObject = go;
        }

        void Start()
        {
            Initialize();
        }

        void OnValidate()
        {
            // Use delayCall to defer Initialize call
            EditorApplication.delayCall += DelayedInitialize;
        }

        void DelayedInitialize()
        {
            if (this == null) return; // Ensure the object still exists
            Initialize();
        }

        void Update()
        {
            if (!Application.isPlaying || sprite != previousSprite)
            {
                Initialize();
                previousSprite = sprite;
            }
        }

        void OnDestroy()
        {
            Cleanup();
        }

        void Cleanup()
        {
            if (mesh != null)
            {
                DestroyImmediate(mesh);
            }

            if (material != null)
            {
                DestroyImmediate(material);
            }
        }

        public void Initialize()
        {
            if (sprite == null)
            {
                LoadDefaultSprite();
            }

            UpdateMesh();
        }

        void LoadDefaultSprite()
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(defaultSpritePath);
        }

        void UpdateMesh()
        {
            // Early exit if sprite or its required properties are null
            if (sprite == null || sprite.vertices == null || sprite.uv == null || sprite.triangles == null)
            {
                return;
            }

            // Create or update material
            if (material == null)
            {
                material = CreateMaterialBasedOnRenderPipeline();
            }

            // Create or update mesh
            if (mesh == null)
            {
                mesh = new Mesh { name = "Generated Mesh" };
            }
            else
            {
                mesh.Clear();
            }

            // Initialize arrays
            Vector3[] vertices = new Vector3[sprite.vertices.Length];
            Vector2[] uvs = new Vector2[sprite.uv.Length];
            int[] triangles = new int[sprite.triangles.Length];

            // Convert sprite data to mesh data
            for (int i = 0; i < sprite.vertices.Length; i++)
            {
                vertices[i] = sprite.vertices[i];  // Assuming sprite.vertices are Vector3
            }

            for (int i = 0; i < sprite.uv.Length; i++)
            {
                uvs[i] = sprite.uv[i];  // Copy UVs
            }

            for (int i = 0; i < sprite.triangles.Length; i++)
            {
                triangles[i] = sprite.triangles[i];  // Copy triangles
            }

            // Assign vertices, UVs, and triangles to the Mesh
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            // Recalculate normals for proper lighting
            mesh.RecalculateNormals();

            // Ensure MeshFilter and MeshRenderer components exist
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            meshRenderer.sharedMaterial = material; // Assign the material to MeshRenderer

            // Assign texture and color to material, only if sprite.texture is not null
            if (sprite.texture != null)
            {
                material.mainTexture = sprite.texture;
            }
            else
            {
                material.mainTexture = null;
            }
            material.color = color;
            material.SetFloat("_Smoothness", 0f);
        }

        void SetMaterialToTransparent(Material mat)
        {
            // Set material to transparent
            mat.SetFloat("_Surface", 1); // SurfaceType.Transparent
            mat.SetFloat("_Blend", 0); // BlendMode.Alpha
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        Material CreateMaterialBasedOnRenderPipeline()
        {
            Material mat;
            var renderPipelineAsset = GraphicsSettings.currentRenderPipeline;

            if (renderPipelineAsset == null)
            {
                mat = new Material(Shader.Find("Standard"))
                {
                    name = "Generated Material"
                };
                SetMaterialToTransparent(mat);
            }
            else if (renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset"))
            {
                mat = new Material(Shader.Find("HDRP/Lit"))
                {
                    name = "Generated Material"
                };
                SetMaterialToTransparent(mat);
            }
            else if (renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset"))
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
                {
                    name = "Generated Material"
                };
                SetMaterialToTransparent(mat);
            }
            else
            {
                Debug.Log("Current render pipeline: Unknown");
                mat = new Material(Shader.Find("Standard"))
                {
                    name = "Generated Material"
                };
                SetMaterialToTransparent(mat);
            }

            return mat;
        }
    }
}
