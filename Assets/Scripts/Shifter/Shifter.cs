using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Shifter
{
    public static readonly Vector2[] directions = { Vector2.right, Vector2.left, Vector2.up, Vector2.down, new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1) };
    public IReadOnlyDictionary<string, ShiftNode> ShiftNodes => _shiftNodes;
    public Vector2 CurrentDirection => _currentDirection;
    public Transform Root => _root;
    public event Action<Vector2> OnShift;
    public event Action<Transform> OnAttached;
    public event Action<Transform> OnDetached;

    private Dictionary<string, ShiftNode> _shiftNodes;
    private List<string> _keys;

    private Transform _root;
    private Vector2 _currentDirection;
    private Vector2 _cashedDirection;
    public Shifter(Transform root, ShiftConfig[] configs)
    {
        _root = root;

        var transformNodes = root.GetComponentsInChildren<Transform>().ToList();
        transformNodes.Remove(root);

        _shiftNodes = new Dictionary<string, ShiftNode>();
        _keys = new List<string>();
        for(var i = 0; i < transformNodes.Count; i++)
        {
            var node = transformNodes[i];
            var path = GetPath(node);
            if (configs[0].ShiftTransformData.ContainsKey(path) == false) continue;
            _shiftNodes[path] = node.TryGetComponent(out SpriteRenderer renderer) ? new ShiftVisualNode(renderer, configs) : new ShiftNode(node, configs);
            _keys.Add(path);
        }   
    }
    public Shifter SetPrimeShift()
    {
        Shift(Vector2.down);
        return this;
    }
    public void Shift(Vector2 direction)
    { 
        if(direction == Vector2.zero) return;

        if ((_cashedDirection - direction).sqrMagnitude < 0.01f) return;

        _cashedDirection = direction;

        var closestDirection = GetClosestDirection(direction.normalized);

        if (closestDirection == _currentDirection) return;

        for (var i = 0; i < _keys.Count; i++)
        {
            var node = _shiftNodes[_keys[i]];
            if (node.Enabled) node.Shift(closestDirection);
        }

        OnShift?.Invoke(closestDirection);

        _currentDirection = closestDirection;
    }
    public void Attach(Transform node, bool includeChildren = true) => InteractWithNode(node, Attach, includeChildren);
    public void Detach(Transform node, bool includeChildren = true) => InteractWithNode(node, Detach, includeChildren);

    private void InteractWithNode(Transform node, Action<ShiftNode> actionWithNode, bool includeChildren)
    {
        if (!includeChildren)
        {
            if (_shiftNodes.TryGetValue(GetPath(node), out var shiftNode)) actionWithNode(shiftNode);
            return;
        }

        var children = node.GetComponentsInChildren<Transform>();

        foreach (var child in children)
        {
            if (_shiftNodes.TryGetValue(GetPath(child), out var shiftNode)) actionWithNode(shiftNode);
        }
    }
    private void Attach(ShiftNode node)
    {
        node.Enabled = true;
        node.Shift(_currentDirection);
        OnAttached?.Invoke(node.Transform);
    }
    private void Detach(ShiftNode node)
    {
        node.Enabled = false;
        OnDetached?.Invoke(node.Transform);
    }
    public static string GetPath(Transform node)
    {
        var result = new StringBuilder(node.name);
        var current = node;
        while(current.parent.name != "root")
        {
            result.Append($"/{current.parent.name}");
            current = current.parent;
        }
        return result.ToString();
    }
    public static Vector2 GetClosestDirection(Vector2 direction)
    {
        var result = directions[0];
        var min = 100f;
        for (var i = 0; i < directions.Length; i++)
        {
            var sqrMagnitude = (direction - directions[i]).sqrMagnitude;
            if (sqrMagnitude < min)
            {
                result = directions[i];
                min = sqrMagnitude;
            }
        }
        return result;

    }
    public static Vector2 GetClosestDirection(Vector2 direction, Vector2[] except)
    {
        if (except != null) return directions.Except(except).OrderBy(i => Vector2.Distance(direction, i)).First();
        else return directions.OrderBy(i => Vector2.Distance(direction, i)).First();
    }
}