#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
public class ShiftBuilder
{
    private Transform _root;
    private Vector2 _direction;

    public void SetRoot(Transform root)
    {
        if(root == null)
        {
            Debug.LogWarning("Root is null");
            return;
        }
        _root = root;
    }
    public ShiftConfig BuildConfig()
    {
        if (_root == null)
        {
            Debug.LogWarning("Root is null");
            return null;
        }
        var children = _root.GetComponentsInChildren<Transform>().ToList();

        if (children == null || children.Count == 0)
        {
            Debug.LogWarning($"Warning! Children is {children}, children count: {children.Count}");
            return null;
        }
        var config = ScriptableObject.CreateInstance<ShiftConfig>();

        children.Remove(_root);

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
        return config;
    }
    public void PrewiewConfig(ShiftConfig config)
    {
        if(config == null)
        {
            Debug.LogWarning("Config is null");
            return;
        }
        if (_root == null)
        {
            Debug.LogWarning("Root is null");
            return;
        }
        config.Deserialize();
        new Shifter(_root, new ShiftConfig[] { config }).SetPrimeShift();
    }
}
#endif
