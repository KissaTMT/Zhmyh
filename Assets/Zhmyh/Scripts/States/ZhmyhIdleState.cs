using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhmyhIdleState : State
{
    private Animator _animator;
    public ZhmyhIdleState(Animator animator)
    {
        _animator = animator;
    }
    public override void Enter()
    {
        _animator.SetBool("IsRun", false);
    }
}
