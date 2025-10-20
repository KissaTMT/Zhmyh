#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ShiftAnimationBuilder
{
    public bool IsPlaying => _isPlaying;
    private ShiftAnimation _animation;
    private ShiftAnimationNode _node;

    private bool _isPlaying = false;
    public void Play()
    {
        _animation.Deserialize();
        if (!_animation.AnimationNodes.TryGetValue(Shifter.GetPath(_node.Transform), out var clip))
        {
            Debug.LogWarning("Animation clip for this node is not exist");
            return;
        }
        if(!_node.Animations.ContainsKey(_animation.name)) _node.AddClip(_animation.name, clip);
        _node.SetAnimation(_animation.name);
        _isPlaying = true;
    }
    public void Stop()
    {
        _isPlaying = false;
    }
    public void SetNode(Transform node, ShiftConfig[] configs = null)
    {
        if(node == null)
        {
            Debug.LogWarning("Node is null");
            return;
        }
        _node = new ShiftAnimationNode(new ShiftNode(node, configs));
    }
    public void SetAnimation(ShiftAnimation animation)
    {
        if(animation == null)
        {
            Debug.LogWarning("Animation is null");
            return;
        }
        _animation = animation;
    }
    public void AddAnimationKey(float timeKey)
    {
        if (_animation == null)
        {
            Debug.LogWarning("Animation is not initialized");
            return;
        }
        if (_node == null)
        {
            Debug.LogWarning("Node is not initialized");
            return;
        }
        _animation.Deserialize();
        AddAnimationKey(_animation.AnimationNodes, new ShiftAnimationData(timeKey,
            _node.Transform.localPosition,
            _node.Transform.localScale,
            _node.Transform.localEulerAngles.z>180? _node.Transform.localEulerAngles.z - 360: _node.Transform.localEulerAngles.z));
        _animation.AnimationNodes[Shifter.GetPath(_node.Transform)] = _animation.AnimationNodes[Shifter.GetPath(_node.Transform)]
            .OrderBy(i => i.TimeKey).ToList();
        _animation.Serialize();
    }
    public void Animate(float timeKey, Vector2 direction)
    {
        if (!_isPlaying) return;
        _node?.Animate(timeKey, direction);
    }
    public void SetAnimationData(float timeKey, Vector2 direction)
    {
        if (!_animation.AnimationNodes.TryGetValue(Shifter.GetPath(_node.Transform), out var clip))
        {
            Debug.LogWarning("Animation clip for this node is not exist");
            return;
        }
        if (!_node.Animations.ContainsKey(_animation.name)) _node.AddClip(_animation.name, clip);
        _node.SetAnimation(_animation.name);
        _node?.SetAnimationData(timeKey, direction);
    }
    private void AddAnimationKey<TValue>(Dictionary<string, List<TValue>> keys, TValue value)
    {
        if(keys.ContainsKey(Shifter.GetPath(_node.Transform))) keys[Shifter.GetPath(_node.Transform)].Add(value);
        else keys[Shifter.GetPath(_node.Transform)] = new List<TValue>() { value};
    }
}
#endif