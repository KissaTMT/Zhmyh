using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShiftVisualNode : ShiftNode
{
    private SpriteRenderer _renderer;

    private Dictionary<Vector2, Sprite> _views;
    public ShiftVisualNode(SpriteRenderer renderer, ShiftConfig[] configs) : base(renderer.GetComponent<Transform>(), configs)
    {
        _renderer = renderer;
        _views = new Dictionary<Vector2, Sprite>();

        for (var i = 0; i < configs.Length; i++)
        {
            _views.Add(configs[i].Direction, configs[i].ShiftVisualData[renderer.name]);
        }

        if (configs.Length < Shifter.directions.Length)
        {
            var exceptDirections = Shifter.directions.Except(_views.Keys).ToArray();

            for (var i = 0; i < exceptDirections.Length; i++)
            {
                _views.Add(exceptDirections[i], _views[Shifter.GetClosestDirection(exceptDirections[i], exceptDirections)]);
            }
        }
    }
    public override void Shift(Vector2 direction)
    {
        base.Shift(direction);
        _renderer.sprite = _views[direction];
    }
}
