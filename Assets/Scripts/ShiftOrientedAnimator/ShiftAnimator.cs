using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShiftAnimator : IDisposable
{
    private Dictionary<string, ShiftAnimation> _animations;
    private Dictionary<string, List<ShiftAnimationNode>> _nodesByAnimation;
    private Dictionary<string, ShiftAnimationNode> _nodes;
    private Shifter _shifter;
    private string _currentAnimation;
    private float _animationProgress;

    private Vector2 _currentDirection;
    private bool _isActive = true;
    private float _speedModifier;
    public ShiftAnimator(Shifter shifter, ShiftAnimation[] animations, float speedModifier = 1)
    {
        _shifter = shifter;
        _nodesByAnimation = new Dictionary<string, List<ShiftAnimationNode>>();
        _animations = new Dictionary<string, ShiftAnimation>();
        _nodes = new Dictionary<string, ShiftAnimationNode>();

        if (animations == null || animations.Length == 0)
        {
            Disable();
            return;
        }
        for (var i = 0; i < animations.Length; i++)
        {
            var anim = animations[i];
            _animations.Add(anim.name, anim);
            var nodeNames = anim.AnimationNodes.Keys.ToList();
            _nodesByAnimation[anim.name] = new List<ShiftAnimationNode>();
            for (var j = 0; j < nodeNames.Count; j++)
            {
                if (!_nodes.ContainsKey(nodeNames[j]))
                {
                    var animationNode = new ShiftAnimationNode(_shifter.ShiftNodes[nodeNames[j]]);
                    _nodes.Add(nodeNames[j], animationNode);
                }
                _nodesByAnimation[anim.name].Add(_nodes[nodeNames[j]]);
                _nodesByAnimation[anim.name][j].AddClip(anim.name, anim.AnimationNodes[nodeNames[j]]);
            }
        }

        _speedModifier = speedModifier;


        _shifter.OnShift += OnShiftHandle;
        _shifter.OnAttached += Attach;
        _shifter.OnDetached += Dettach;
    }
    public void Dispose()
    {
        _shifter.OnShift -= OnShiftHandle;
        _shifter.OnAttached -= Attach;
        _shifter.OnDetached -= Dettach;
    }
    public void Enable() => _isActive = true;
    public void Disable() => _isActive = false;
    public void Tick()
    {
        if (!_isActive) return;

        var nodes = _nodesByAnimation[_currentAnimation];

        _animationProgress = (_animationProgress + Time.deltaTime * _animations[_currentAnimation].PlaybackSpeed * _speedModifier) % 1;

        var count = nodes.Count;
        for (var i = 0; i < count; i++)
        {
            if (nodes[i].Enabled) nodes[i].Animate(_animationProgress, _currentDirection);
        }
    }
    public void OnShiftHandle(Vector2 direction) => _currentDirection = direction;
    public void SetAnimation(string name)
    {
        if (!_isActive) return;
        _currentAnimation = name;
        var nodes = _nodesByAnimation[name];
        for (var i = 0; i < nodes.Count; i++)
        {
            nodes[i].SetAnimation(_currentAnimation);
        }
    }
    public void Attach(Transform node)
    {
        if(_nodes.TryGetValue(Shifter.GetPath(node), out var animationNode)) animationNode.Enabled = true;
    }
    public void Dettach(Transform node)
    {
        if (_nodes.TryGetValue(Shifter.GetPath(node), out var animationNode)) animationNode.Enabled = false;
    }

}
