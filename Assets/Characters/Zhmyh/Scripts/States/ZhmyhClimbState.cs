﻿using System;
using UnityEngine;

public class ZhmyhClimbState : State
{
    public ClimbController ClimbController => _climbController;
    private Vector2 _direction;
    private ClimbController _climbController;
    private ShiftAnimator _shiftAnimator;
    public ZhmyhClimbState(Transform transform, ShiftAnimator shiftAnimator,Shifter shifter, float climbForce)
    {
        _climbController = new ClimbController(transform.parent.parent, transform, climbForce);
        _shiftAnimator = shiftAnimator;

        Run += Climb;
    }
    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }
    public override void Enter()
    {
        Run = ReloadRun;
    }
    public override void ReloadRun()
    {
        Climb();
    }
    public void Climb()
    {
        _climbController.Climb(_direction);
    }
}
