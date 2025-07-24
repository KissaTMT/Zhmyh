using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Shifter
{
    public static readonly Vector2[] directions = { Vector2.right, Vector2.left, new Vector2(1, 1), new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1) };
    public IReadOnlyDictionary<string, ShiftTransformNode> ShiftNodes => _shiftNodes;
    public Vector2 CurrentDirection => _currentDirection;
    public Transform Root => _root;
    public event Action<Vector2> OnShift;
    public event Action<Transform> OnAttached;
    public event Action<Transform> OnDetached;

    private Dictionary<string, ShiftTransformNode> _shiftNodes;

    private Transform _root;
    private Vector2 _currentDirection;
    public Shifter(Transform root, ShiftConfig[] configs)
    {
        _root = root;

        var transformNodes = root.GetComponentsInChildren<Transform>().ToList();
        transformNodes.Remove(root);

        _shiftNodes = new Dictionary<string, ShiftTransformNode>();
        for(var i = 0; i < transformNodes.Count; i++)
        {
            var node = transformNodes[i];
            _shiftNodes[GetPath(node)] = new ShiftTransformNode(node, configs);
        }   
    }
    public Shifter SetPrimeShift()
    {
        Shift(new Vector2(1, -1));
        return this;
    }
    public void Shift(Vector2 direction)
    {
        if(direction == Vector2.zero) return;

        var approximateDirection = GetClosestDirection(direction);

        if (approximateDirection == _currentDirection) return;

        foreach (var key in _shiftNodes.Keys)
        {
            var node = _shiftNodes[key];
            if(node.Enabled) node.Shift(approximateDirection);
        }

        OnShift?.Invoke(approximateDirection);
        _currentDirection = approximateDirection;
    }
    public void Attach(Transform transform)
    {
        //Debug.Log("Attach");
        if (_shiftNodes.TryGetValue(GetPath(transform), out var shiftNode))
        {
            shiftNode.Enabled = true;
            shiftNode.Shift(_currentDirection);
        }
        OnAttached?.Invoke(transform);
    }
    public void Detach(Transform transform)
    {
        //Debug.Log("Detach");
        if (_shiftNodes.TryGetValue(GetPath(transform), out var shiftNode)) shiftNode.Enabled = false;
        OnDetached?.Invoke(transform);
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
    public static Vector2 GetClosestDirection(Vector2 direction, Vector2[] except = null)
    {
        if (except != null) return directions.Except(except).OrderBy(i => Vector2.Distance(direction, i)).First();
        else return directions.OrderBy(i => Vector2.Distance(direction, i)).First();
    }
}