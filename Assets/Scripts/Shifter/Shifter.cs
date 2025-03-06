using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shifter
{
    private const int MAX_VIEW_COUNT = 6;

    public event Action OnShifted;
    public event Action OnAttached;
    public event Action OnDetached;
    public Vector2 FacingDirection => _facingDirection;

    private readonly Vector2[] _directions = {Vector2.right, Vector2.left, new Vector2(1,1), new Vector2(-1,-1), new Vector2(1,-1),new Vector2(-1,1)};

    private Dictionary<Vector2, ShiftConfig> _views;
    private SpriteRenderer[] _nodes;
    private Vector2 _facingDirection;
    private int _order;
    public Shifter(Transform root, ShiftConfig[] configs)
    {
        _nodes = root.GetComponentsInChildren<SpriteRenderer>();
        _views = new Dictionary<Vector2, ShiftConfig>();

        _order = configs.Length;

        for (var i = 0; i < configs.Length; i++)
        {
            _views.Add(configs[i].Direction, configs[i]);
        }
        if(_order < MAX_VIEW_COUNT)
        {
            var keys = _directions.Except(_views.Keys).ToArray();

            for(var i=0;i<keys.Length;i++)
            {
                _views.Add(keys[i], _views[Interpolate(keys[i], keys)]);
            }
        }
    }
    public void Shift(Vector2 direction)
    {
        if(direction == Vector2.zero) return;

        var facingDirection = Interpolate(direction);

        for (var i = 0; i < _views[facingDirection].Sprites.Count; i++)
        {
            var view = _views[facingDirection];
            _nodes[i].sprite = view.Sprites[i];
            _nodes[i].transform.localPosition = view.LocalPositions[i];
            _nodes[i].transform.localScale = view.LocalScales[i];
            _nodes[i].transform.eulerAngles = view.EulerAngles[i];
        }
        OnShifted?.Invoke();
        _facingDirection = facingDirection;
    }
    public void Attach(Transform transform, Action action)
    {
        OnAttached?.Invoke();
    }
    public void Detach(Transform transform)
    {
        OnDetached?.Invoke();
    }
    private Vector2 Interpolate(Vector2 direction, Vector2[] except = null)
    {
        if (except != null) return _directions.Except(except).OrderBy(i => Vector2.Distance(direction, i)).ToArray()[0];
        else return _directions.OrderBy(i => Vector2.Distance(direction, i)).ToArray()[0];
    }
}