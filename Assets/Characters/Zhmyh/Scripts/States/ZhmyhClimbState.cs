using System;
using UnityEngine;

public class ZhmyhClimbState : State
{
    public SurfaceClimber ClimbController => _climbController;
    private Vector2 _direction;
    private SurfaceClimber _climbController;
    private ShiftAnimator _shiftAnimator;
    public ZhmyhClimbState(Transform transform, ShiftAnimator shiftAnimator,Shifter shifter, float climbForce)
    {
        _climbController = new SurfaceClimber(transform, climbForce);
        _shiftAnimator = shiftAnimator;
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }
    public override void OnEnter()
    {

    }
    public override void OnTick()
    {
        Climb();
    }
    public void Climb()
    {
        _climbController.Climb(_direction);
    }
}
