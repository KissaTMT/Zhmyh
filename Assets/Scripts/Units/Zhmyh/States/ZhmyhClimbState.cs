using System;
using UnityEngine;

public class ZhmyhClimbState : State
{
    public SurfaceClimber Climber => _climber;
    private Vector2 _direction;
    private SurfaceClimber _climber;
    private ShiftAnimator _shiftAnimator;
    public ZhmyhClimbState(Transform transform, ShiftAnimator shiftAnimator,Shifter shifter, float climbForce)
    {
        _climber = new SurfaceClimber(transform, climbForce);
        _shiftAnimator = shiftAnimator;
    }
    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }
    public override void OnTick()
    {
        Climb();
    }
    public void Climb()
    {
        _climber.Climb(_direction);
    }
}
