using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ShiftNode
{
    public bool Enabled {  get; set; }
    public ShiftTransformData CurrentView => currentView;
    public Transform Transform => transform;
    public Vector2 CurrentDirection => currentDirection;

    protected Transform transform;
    protected Vector2 currentDirection;
    protected ShiftTransformData currentView;

    private Dictionary<Vector2, ShiftTransformData> _views;
   

    public ShiftNode(Transform transform, ShiftConfig[] configs = null)
    {
        Enabled = true;

        _views = new Dictionary<Vector2, ShiftTransformData>();
        this.transform = transform;

        if (configs == null) throw new ArgumentNullException("Configs is null");

        for (var i = 0; i < configs.Length; i++)
        {
            _views.Add(configs[i].Direction, configs[i].ShiftTransformData[Shifter.GetPath(this.transform)]);
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
    public virtual void Shift(Vector2 direction)
    { 
        transform.localPosition = _views[direction].Position;
        transform.localScale = _views[direction].Scale;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 
            transform.localEulerAngles.y,
            _views[direction].Angle);

        currentDirection = direction;
        currentView = _views[direction];
    }
}
