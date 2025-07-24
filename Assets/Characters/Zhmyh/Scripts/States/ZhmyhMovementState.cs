using System;
using UnityEngine;

public class ZhmyhMovementState : State
{
    private IMovementController _movement;
    private Shifter _shifter;

    private ShiftAnimator _shiftAnimator;
    private Vector2 _direction;
    public ZhmyhMovementState(Transform transform, ShiftAnimator shiftAnimator, Shifter shifter,float speed)
    {
        _shiftAnimator = shiftAnimator;
        _movement = new NPhMovementController(transform,speed);
        _shifter = shifter;
    }
    public override void Enter()
    {
        _shiftAnimator.SetAnimation("walk");
        Run = ReloadRun;
    }
    public override void ReloadRun()
    {
        Move();
        Shift();
    }
    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }
    public void Move() => _movement.Move(_direction);
    public void Shift() => _shifter.Shift(_direction);
}
