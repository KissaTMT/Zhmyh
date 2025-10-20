#if UNITY_EDITOR

using R3;
using System;
using UnityEditor;
using UnityEngine;

public class ShiftBuilderWindow : EditorWindow
{
    private ReactiveProperty<Transform> Root = new();
    private Vector2 _direction;
    private ShiftConfig _preview;
    private CompositeDisposable _disposables = new();

    private ShiftBuilder _builder = new();

    [MenuItem("Tools/Shift Builder")]
    public static void ShowWindow()
    {
        GetWindow<ShiftBuilderWindow>("Shift Builder");
    }
    private void OnEnable()
    {
        Root.Subscribe(OnRootChanged).AddTo(_disposables);
    }
    private void OnDisable()
    {
        _disposables.Dispose();
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
        var assetPath = $"Assets/Characters/{Root.Value.parent.name}/ShiftConfigs/{_direction} {Root.Value.parent.name}.asset";
        _builder.SetDirection(_direction);
        var config = _builder.BuildConfig();
        if (config == null) return;
        if (AssetDatabase.LoadAssetAtPath<ShiftConfig>(assetPath)) AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        _preview = config;
    }
}
#endif