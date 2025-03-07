using System;
using UnityEngine;

public class ZhmyhClimbState : State
{
    public ClimbController ClimbController => _climbController;
    private Vector2 _direction;
    private ClimbController _climbController;
    private Animator _animator;
    public ZhmyhClimbState(Transform transform, Animator animator,Shifter shifter, float climbForce)
    {
        _climbController = new ClimbController(transform.parent.parent, transform, climbForce);
        _animator = animator;

        Run += Climb;
    }
    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }
    public override void Enter()
    {
        _animator.SetBool("IsClimbing", true);
        Run = ReloadRun;
    }

    public override void Exit()
    {
        _animator.SetBool("IsClimbing", false);
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
