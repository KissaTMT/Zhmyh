using System;
using UnityEngine;

public class ZhmyhMovementState : State
{
    private IMovementController _movement;
    private Shifter _shifter;

    private Animator _animator;
    private Vector2 _direction;
    public ZhmyhMovementState(Transform transform, Animator animator, Shifter shifter,float speed)
    {
        _animator = animator;
        _movement = new NPhMovementController(transform,speed);
        _shifter = shifter;
    }
    public override void Enter()
    {
        _animator.SetBool("IsRun", true);
        Run = ReloadRun;
    }
    public override void Exit() => _animator.SetBool("IsRun", false);
    public override void ReloadRun()
    {
        Move();
        Shift();
    }
    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
        _animator.SetFloat("X", _direction.x);
        _animator.SetFloat("Y", _direction.y);
    }
    public void Move() => _movement.Move(_direction);
    public void Shift() => _shifter.Shift(_direction);
}
