using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ShiftTransformNode
{
    public bool Enabled {  get; set; }
    public Transform Transform => _transform;
    public Vector2 CurrentDirection => _currentDirection;

    private Transform _transform;

    private Dictionary<Vector2, ShiftTransformData> _views;
    private Vector2 _currentDirection;

    public ShiftTransformNode(Transform transform, ShiftConfig[] configs)
    {
        Enabled = true;

        _views = new Dictionary<Vector2, ShiftTransformData>();
        _transform = transform;

        for (var i = 0; i < configs.Length; i++)
        {
            _views.Add(configs[i].Direction, configs[i].ShiftTransformData[Shifter.GetPath(_transform)]);
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
    public void Shift(Vector2 direction)
    { 
        _transform.localPosition = _views[direction].Position;
        _transform.localScale = _views[direction].Scale;
        _transform.localEulerAngles = new Vector3(_transform.localEulerAngles.x, 
            _transform.localEulerAngles.y,
            _views[direction].Angle);

        _currentDirection = direction;
    }
}
