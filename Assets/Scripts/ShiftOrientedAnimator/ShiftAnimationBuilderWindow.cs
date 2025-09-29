#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public class ShiftAnimationBuilderWindow : EditorWindow
{
    private ReactiveProperty<ShiftAnimation> Animation = new();
    private ReactiveProperty<Transform> SelectedNode = new();
    private ReactiveProperty<ShiftConfig> ShiftConfig = new();

    private ShiftAnimationBuilder _builder = new();
    private Vector2 _direction = new (1, -1);
    private float _timeKey = 0f;

    [MenuItem("Tools/Shift Animation Builder")]
    public static void ShowWindow()
    {
        GetWindow<ShiftAnimationBuilderWindow>("Shift Animation Builder");
    }
    private void OnEnable()
    {
        Animation.OnChanged += OnAnimationChanged;
        SelectedNode.OnChanged += OnNodeChanged;
        ShiftConfig.OnChanged += OnShiftConfigChanged;
    }

    private void OnDisable()
    {
        Animation.OnChanged -= OnAnimationChanged;
        SelectedNode.OnChanged -= OnNodeChanged;
        ShiftConfig.OnChanged -= OnShiftConfigChanged;
    }

    private void OnGUI()
    {
        GUILayout.Label("Shift Animation Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        Animation.Value = (ShiftAnimation)EditorGUILayout.ObjectField("Animation Asset", Animation.Value, typeof(ShiftAnimation), false);

        EditorGUILayout.Space();

        SelectedNode.Value = (Transform)EditorGUILayout.ObjectField("Node", SelectedNode.Value, typeof(Transform), true);

        EditorGUILayout.Space();

        ShiftConfig.Value = (ShiftConfig)EditorGUILayout.ObjectField("ShiftConfig Asset", ShiftConfig.Value, typeof(ShiftConfig), false);

        EditorGUILayout.Space();

        _timeKey = EditorGUILayout.Slider("Time Key", _timeKey, 0f, 1f);

        EditorGUILayout.Space();

        _direction = EditorGUILayout.Vector2Field("Direction", _direction);

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play"))
        {
            Play();
        }
        if (GUILayout.Button("Stop"))
        {
            Stop();
        }

        if (GUILayout.Button("Add Key"))
        {
            AddAnimationKey();
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (SelectedNode.Value != null)
        {
            EditorGUILayout.LabelField("Selected Node:", SelectedNode.Value.name);
            EditorGUILayout.LabelField("Node Path:", Shifter.GetPath(SelectedNode.Value));

            if (Animation.Value != null)
            {
                Animation.Value.Deserialize();
                string nodePath = Shifter.GetPath(SelectedNode.Value);

                if (Animation.Value.AnimationNodes.ContainsKey(nodePath))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("Animation Keys:", EditorStyles.boldLabel);

                    var keys = Animation.Value.AnimationNodes[nodePath];
                    for (int i = 0; i < keys.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Key {i}: Time = {keys[i].TimeKey:F2}");
                        if (GUILayout.Button("Go", GUILayout.Width(40)))
                        {
                            _timeKey = keys[i].TimeKey;
                            _builder.SetAnimationData(_timeKey, _direction);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void Stop()
    {
        _builder.Stop();
        EditorApplication.update -= AnimatePreview;
    }

    private void OnAnimationChanged(ShiftAnimation animation)
    {
        _builder.SetAnimation(animation);
    }

    private void OnNodeChanged(Transform transform)
    {
        _builder.SetNode(transform);
    }
    private void OnShiftConfigChanged(ShiftConfig config)
    {
        
    }

    private void Play()
    {
        if (Animation.Value != null)
        {
            if (_builder.IsPlaying) return;
            _builder.Play();
            Debug.Log("Playing animation: " + Animation.Value.name);

            EditorApplication.update += AnimatePreview;
        }
        else
        {
            Debug.LogWarning("No animation selected!");
        }
    }

    private void AnimatePreview()
    {
        if (Animation.Value == null)
        {
            EditorApplication.update -= AnimatePreview;
            return;
        }
        _timeKey = (_timeKey + Time.deltaTime * Animation.Value.PlaybackSpeed) % 1;

        _builder.Animate(_timeKey,_direction);
        Repaint();
    }

    private void AddAnimationKey()
    {
        _builder.AddAnimationKey(_timeKey);

        EditorUtility.SetDirty(Animation.Value);
        AssetDatabase.SaveAssets();

        Debug.Log("Added key for node: " + SelectedNode.Value.name + " at time: " + _timeKey);
    }
}

#endif