using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhmyhIdleState : State
{
    private ShiftAnimator _shiftAnimator;
    public ZhmyhIdleState(ShiftAnimator shiftAnimator)
    {
        _shiftAnimator = shiftAnimator;
    }
    public override void Enter()
    {
        _shiftAnimator.SetAnimation("idle");
    }
}
