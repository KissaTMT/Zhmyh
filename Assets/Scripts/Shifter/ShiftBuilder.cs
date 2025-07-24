using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.STP;

#if UNITY_EDITOR
public class ShiftBuilder : MonoBehaviour
{
    [SerializeField] private Vector2 _direction;
    [SerializeField] private ShiftConfig _preview;

    [Button("Build Config")]
    private void BuildConfig()
    {
        var children = GetComponentsInChildren<Transform>().ToList();

        if (children == null || children.Count == 0) return;
        var assetPath = $"Assets/Characters/{transform.parent.name}/ShiftConfigs/{_direction}.asset";
        var config = ScriptableObject.CreateInstance<ShiftConfig>();

        children.Remove(transform);

        config.Direction = _direction;
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            config.ShiftTransformData[Shifter.GetPath(child)] = new ShiftTransformData(Shifter.GetPath(child),
                child.localPosition, 
                child.localScale, 
                child.localEulerAngles.z);
            if (child.TryGetComponent(out SpriteRenderer renderer)) config.ShiftVisualData[child.name] = renderer.sprite;
        }
        config.Serialize();
        if(AssetDatabase.LoadAssetAtPath<ShiftConfig>(assetPath)) AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        _preview = config;
    }
    [Button("Preview Config")]
    private void PrewiewConfig()
    {
        _preview.Deserialize();
        new Shifter(transform, new ShiftConfig[] { _preview }).SetPrimeShift();
    }
}
#endif
