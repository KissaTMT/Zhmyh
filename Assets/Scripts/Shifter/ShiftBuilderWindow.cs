#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class ShiftBuilderWindow : EditorWindow
{
    private ReactiveProperty<Transform> Root = new();
    private Vector2 _direction;
    private ShiftConfig _preview;

    private ShiftBuilder _builder = new();

    [MenuItem("Tools/Shift Builder")]
    public static void ShowWindow()
    {
        GetWindow<ShiftBuilderWindow>("Shift Builder");
    }
    private void OnEnable()
    {
        Root.OnChanged += OnRootChanged;
    }
    private void OnDisable()
    {
        Root.OnChanged -= OnRootChanged;
    }
    private void OnGUI()
    {
        GUILayout.Label("Shift Builder", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        Root.Value = (Transform)EditorGUILayout.ObjectField("Root", Root.Value, typeof(Transform), true);

        EditorGUILayout.Space();

        _direction = EditorGUILayout.Vector2Field("Direction", _direction);

        EditorGUILayout.Space();

        _preview = (ShiftConfig)EditorGUILayout.ObjectField("Preview Shift Asset", _preview, typeof(ShiftConfig), false);
        
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Build Config"))
        {
            Build();
        }
        if (GUILayout.Button("Preview Config"))
        {
            Preview();
        }

        GUILayout.EndHorizontal();
    }
    private void OnRootChanged(Transform root)
    {
        _builder.SetRoot(root);
    }
    private void Preview()
    {
        _builder.PrewiewConfig(_preview);
    }

    private void Build()
    {
        var assetPath = $"Assets/Characters/{Root.Value.parent.name}/ShiftConfigs/{_direction}.asset";
        var config = _builder.BuildConfig();
        if (config == null) return;
        if (AssetDatabase.LoadAssetAtPath<ShiftConfig>(assetPath)) AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        _preview = config;
    }
}
#endif