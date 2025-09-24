#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

public class ShiftAnimationBuilderWindow : EditorWindow
{
    public float TimeKey
    {
        get => _timeKey;
        set
        {
            _timeKey = value;
            Preview();
        }
    }
    private ShiftAnimation _animation;
    private Transform _selectedNode;
    private float _timeKey = 0f;
    private int _index = 0;

    private ShiftAnimationNode _previewNode;

    [MenuItem("Tools/Shift Animation Builder")]
    public static void ShowWindow()
    {
        GetWindow<ShiftAnimationBuilderWindow>("Animation Builder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Shift Animation Builder", EditorStyles.boldLabel);

        // Поле для выбора анимации
        _animation = (ShiftAnimation)EditorGUILayout.ObjectField("Animation", _animation, typeof(ShiftAnimation), false);

        // Поле для выбора ноды (можно выбирать из сцены)
        _selectedNode = (Transform)EditorGUILayout.ObjectField("Node", _selectedNode, typeof(Transform), true);

        // Слайдер для времени
        TimeKey = EditorGUILayout.Slider("Time Key", TimeKey, 0f, 1f);

        EditorGUILayout.Space();

        // Кнопки управления
        if (GUILayout.Button("Play Animation"))
        {
            Play();
        }

        if (GUILayout.Button("Add Animation Key"))
        {
            AddAnimationKey();
        }

        EditorGUILayout.Space();

        // Информация о выбранной ноде
        if (_selectedNode != null)
        {
            EditorGUILayout.LabelField("Selected Node:", _selectedNode.name);
            EditorGUILayout.LabelField("Node Path:", Shifter.GetPath(_selectedNode));
        }
    }
    private void Preview()
    {
        _previewNode?.Animate(TimeKey, new Vector2(1,-1));
    }
    private void Play()
    {
        if (_animation != null)
        {
            _animation.Deserialize();
            _previewNode = new ShiftAnimationNode(_selectedNode);
            _previewNode.AddClip(_animation.name, _animation.AnimationNodes[Shifter.GetPath(_selectedNode)]);
            _previewNode.SetAnimation(_animation.name);

            Debug.Log("Playing animation: " + _animation.name);
        }
        else
        {
            Debug.LogWarning("No animation selected!");
        }
    }

    private void AddAnimationKey()
    {
        if (_selectedNode == null)
        {
            Debug.LogWarning("Please select a node first!");
            return;
        }

        if (_animation == null)
        {
            Debug.LogWarning("Please select an animation first!");
            return;
        }

        _animation.Deserialize();

        var animationData = new ShiftAnimationData(
            TimeKey,
            _selectedNode.localPosition,
            _selectedNode.localScale,
            _selectedNode.localEulerAngles.z > 180 ?
                _selectedNode.localEulerAngles.z - 360 : _selectedNode.localEulerAngles.z
        );

        string nodePath = Shifter.GetPath(_selectedNode);

        if (!_animation.AnimationNodes.ContainsKey(nodePath))
        {
            _animation.AnimationNodes[nodePath] = new List<ShiftAnimationData>();
        }

        _animation.AnimationNodes[nodePath].Add(animationData);
        _animation.AnimationNodes[nodePath] = _animation.AnimationNodes[nodePath]
            .OrderBy(i => i.TimeKey).ToList();

        _animation.Serialize();

        Debug.Log("Added key for node: " + _selectedNode.name + " at time: " + TimeKey);
    }

    private void NextKey()
    {
        if (_selectedNode == null || _animation == null)
        {
            Debug.LogWarning("Please select both an animation and a node first!");
            return;
        }

        _animation.Deserialize();
        string nodePath = Shifter.GetPath(_selectedNode);

        if (!_animation.AnimationNodes.ContainsKey(nodePath) ||
            _animation.AnimationNodes[nodePath].Count == 0)
        {
            Debug.LogWarning("No animation keys found for selected node!");
            return;
        }

        var keys = _animation.AnimationNodes[nodePath];
        var key = keys[_index];

        _selectedNode.localPosition = new Vector3(key.PositionKey.x, key.PositionKey.y, _selectedNode.localPosition.z);
        _selectedNode.localEulerAngles = new Vector3(0, 0, key.AngleKey);

        _index = (_index + 1) % keys.Count;

        Debug.Log("Showing key " + _index + " of " + keys.Count);
    }
}

#endif