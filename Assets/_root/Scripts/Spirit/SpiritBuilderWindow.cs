#if UNITY_EDITOR
using Spirit;
using UnityEditor;
using UnityEngine;

namespace Spirit
{
    public class SpiritBuilderWindow : EditorWindow
    {
        private SpiritSkeletonBuilder _skeletonBuilder = new();
        private SpiritMeshBuilder _meshBuilder = new();
        private SpiritConfigGenerator _configGenerator = new();
        private SpiritSMRBuilder _smrBuilder = new();

        private Transform _root;
        private SpiritConfig _config;
        private SpiritConfig _referenceConfig;
        private Mesh _lastMesh;
        private GameObject _lastSkeleton;

        private string _meshSavePath = "Assets/_root/Meshes/Character.asset";
        private string _configSavePath = "Assets/Configs/Character_Config.asset";

        private string _lastSavedMeshPath;
        private string _lastSavedConfigPath;

        private int _gridX = 4;
        private int _gridY = 4;

        [MenuItem("Tools/Spirit Builder")]
        public static void ShowWindow() => GetWindow<SpiritBuilderWindow>("Spirit Builder");

        private void OnGUI()
        {
            GUILayout.Label("Spirit Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _root = (Transform)EditorGUILayout.ObjectField("Root", _root, typeof(Transform), true);

            EditorGUILayout.Space();

            _referenceConfig = (SpiritConfig)EditorGUILayout.ObjectField("Reference config", _referenceConfig, typeof(SpiritConfig), true);

            EditorGUILayout.Space();

            _meshSavePath = EditorGUILayout.TextField("Mesh Save Path", _meshSavePath);
            if (GUILayout.Button("Select Mesh Path"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Mesh", "SpiritMesh", "asset", "Choose location");
                if (!string.IsNullOrEmpty(path)) _meshSavePath = path;
            }

            _configSavePath = EditorGUILayout.TextField("Config Save Path", _configSavePath);
            if (GUILayout.Button("Select Config Path"))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Config", "SpiritConfig", "asset", "Choose location");
                if (!string.IsNullOrEmpty(path)) _configSavePath = path;
            }

            EditorGUILayout.Space();

            _gridX = EditorGUILayout.IntField("Grid X (cells)", _gridX);
            _gridY = EditorGUILayout.IntField("Grid Y (cells)", _gridY);
            _gridX = Mathf.Max(2, _gridX);
            _gridY = Mathf.Max(2, _gridY);

            EditorGUILayout.Space();

            // ---- 4 КНОПКИ ----
            if (GUILayout.Button("1. Create Skeleton", GUILayout.Height(30)))
            {
                CreateSkeleton();
            }

            if (GUILayout.Button("2. Build Mesh", GUILayout.Height(30)))
            {
                BuildMesh();
            }

            if (GUILayout.Button("3. Build Config", GUILayout.Height(30)))
            {
                BuildConfig();
            }

            if (GUILayout.Button("4. Create SMR in Scene", GUILayout.Height(30)))
            {
                CreateSMR();
            }

            EditorGUILayout.Space();

            // ---- Инфо ----
            if (_lastSkeleton != null)
                EditorGUILayout.HelpBox($"Skeleton: {_lastSkeleton.name}", MessageType.Info);

            if (!string.IsNullOrEmpty(_lastSavedMeshPath))
                EditorGUILayout.HelpBox($"Mesh: {_lastSavedMeshPath}", MessageType.Info);

            if (!string.IsNullOrEmpty(_lastSavedConfigPath))
                EditorGUILayout.HelpBox($"Config: {_lastSavedConfigPath}", MessageType.Info);
        }

        private void CreateSkeleton()
        {
            if (_root == null)
            {
                EditorUtility.DisplayDialog("Error", "Root is null!", "OK");
                return;
            }

            _skeletonBuilder.SetRoot(_root);
            _lastSkeleton = _skeletonBuilder.BuildSkeleton();

            if (_lastSkeleton != null)
                EditorUtility.DisplayDialog("Success", "Skeleton created!", "OK");
        }

        private void BuildMesh()
        {
            if (_root == null)
            {
                EditorUtility.DisplayDialog("Error", "Root is null!", "OK");
                return;
            }

            _meshBuilder.SetRoot(_root);
            _meshBuilder.SetSkinned(_lastSkeleton);
            Mesh mesh = _meshBuilder.BuildGridMesh(_gridX, _gridY);

            if (mesh == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to build mesh.", "OK");
                return;
            }

            _lastSavedMeshPath = _meshBuilder.SaveMeshAsset(mesh, _meshSavePath);
            _lastMesh = mesh;

            if (!string.IsNullOrEmpty(_lastSavedMeshPath))
            {
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Mesh>(_lastSavedMeshPath));
                EditorUtility.DisplayDialog("Success", $"Mesh saved!", "OK");
            }
        }

        private void BuildConfig()
        {
            if (_root == null)
            {
                EditorUtility.DisplayDialog("Error", "Root is null!", "OK");
                return;
            }

            // Используем последний меш или строим временный
            Mesh tempMesh = _lastMesh;
            if (tempMesh == null)
            {
                _meshBuilder.SetRoot(_root);
                tempMesh = _meshBuilder.BuildGridMesh(_gridX, _gridY);
                if (tempMesh == null)
                {
                    EditorUtility.DisplayDialog("Error", "Failed to build temporary mesh.", "OK");
                    return;
                }
            }

            _configGenerator.SetRoot(_root);
            _configGenerator.SetReference(_referenceConfig);
            SpiritConfig config = _configGenerator.GenerateConfig(tempMesh,_gridX,_gridY);

            if (config == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to generate config.", "OK");
                return;
            }

            _lastSavedConfigPath = _configGenerator.SaveConfigAsset(config, _configSavePath);
            _config = config;

            if (!string.IsNullOrEmpty(_lastSavedConfigPath))
            {
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SpiritConfig>(_lastSavedConfigPath));
                EditorUtility.DisplayDialog("Success", $"Config saved!", "OK");
            }
        }

        private void CreateSMR()
        {
            if (_root == null)
            {
                EditorUtility.DisplayDialog("Error", "Root is null!", "OK");
                return;
            }

            if (_lastSkeleton == null)
            {
                EditorUtility.DisplayDialog("Error", "No skeleton found. Create Skeleton first!", "OK");
                return;
            }

            if (_lastMesh == null && string.IsNullOrEmpty(_lastSavedMeshPath))
            {
                EditorUtility.DisplayDialog("Error", "No mesh found. Build Mesh first!", "OK");
                return;
            }

            Mesh mesh = _lastMesh ?? AssetDatabase.LoadAssetAtPath<Mesh>(_lastSavedMeshPath);
            if (mesh == null)
            {
                EditorUtility.DisplayDialog("Error", "Cannot load mesh!", "OK");
                return;
            }

            SpiritConfig config = _config ?? AssetDatabase.LoadAssetAtPath<SpiritConfig>(_lastSavedConfigPath);

            _smrBuilder.SetRoot(_root);
            _smrBuilder.SetMesh(mesh);
            _smrBuilder.SetConfig(config);
            _smrBuilder.SetSkeleton(_lastSkeleton);

            GameObject result = _smrBuilder.BuildSMR();

            if (result != null)
            {
                Selection.activeGameObject = result;
                EditorUtility.DisplayDialog("Success", $"SMR created on {result.name}!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to create SMR.", "OK");
            }
        }
    }
}
#endif