using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

public class ShiftBuilder : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Vector2 _direction;
    [SerializeField] private ShiftConfig _preview;
    [Button("Build Config")]
    private void BuildConfig()
    {
        var children = GetComponentsInChildren<SpriteRenderer>();

        if (children == null || children.Length == 0) return;
        var assetPath = $"Assets/{transform.parent.name}/ShiftConfigs/{_direction}.asset";
        var config = ScriptableObject.CreateInstance<ShiftConfig>();

        config.Direction = _direction;
        for (var i = 0; i < children.Length; i++)
        {
            config.Sprites.Add(children[i].sprite);
            config.LocalPositions.Add(children[i].transform.localPosition);
            config.LocalScales.Add(children[i].transform.localScale);
            config.EulerAngles.Add(children[i].transform.eulerAngles);
        }
        if(AssetDatabase.LoadAssetAtPath<ShiftConfig>(assetPath)) AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        _preview = config;
    }
    [Button("Preview Config")]
    private void PrewiewConfig()
    {
        var children = GetComponentsInChildren<SpriteRenderer>();
        if (children == null || children.Length == 0) return;

        for (var i = 0; i < children.Length; i++)
        {
            children[i].sprite = _preview.Sprites[i];
            children[i].transform.localPosition = _preview.LocalPositions[i];
            children[i].transform.localScale = _preview.LocalScales[i];
            children[i].transform.eulerAngles = _preview.EulerAngles[i];
        }
    }
#endif
}
