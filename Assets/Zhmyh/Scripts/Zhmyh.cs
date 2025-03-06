using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zhmyh : MonoBehaviour
{
    [SerializeField] private Transform _bow;
    [SerializeField] private Transform _arrow;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _climbSpeed;
    [SerializeField] private ShiftConfig[] _configs;
    [SerializeField] private GameObject _aim;
    [SerializeField] private Transform _body;
    
    private Transform _transform;
    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private Shifter _shifter;
    private StateMachine _sm;

    private Vector2 _currentDirection;
    private Vector2 _aimDirection = Vector2.zero;

    private bool _isClimbing = false;
    private bool _isAiming = false;

    private ZhmyhIdleState _idleState;
    private ZhmyhMovementState _movementState;
    private ZhmyhClimbState _climbState;
    private ZhmyhAimingState _aimingState;
    public Zhmyh Init()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _shifter = new Shifter(_transform, _configs);

        _sm = new StateMachine();

        _idleState = new ZhmyhIdleState(_animator);
        _movementState = new ZhmyhMovementState(_transform, _animator, _shifter,_movementSpeed);
        _climbState = new ZhmyhClimbState(_body, _animator, _shifter, _climbSpeed);
        _aimingState = new ZhmyhAimingState(transform, _aim.transform,_shifter);

        _sm.AddTransition(_idleState, _movementState, () => _currentDirection.magnitude > 0.1f);
        _sm.AddTransition(_movementState, _idleState, () => _currentDirection.magnitude < 0.1f);
        _sm.AddAnyTransition(_climbState, () => _isClimbing == true);
        _sm.AddTransition(_climbState, _idleState, () => _isClimbing == false);
        _sm.AddTransition(_idleState, _aimingState, () => _isAiming == true);
        _sm.AddTransition(_movementState, _aimingState, () => _isAiming == true, _movementState.Move);
        _sm.AddTransition(_aimingState, _movementState, () => _isAiming == false && _currentDirection.magnitude > 0.1f);
        _sm.AddTransition(_aimingState, _idleState, () => _isAiming == false && _currentDirection.magnitude < 0.1f);

        _sm.SetState(_idleState);

        return this;
    }
    public void SetDirection(Vector2 input)
    {
        _currentDirection = input;
        _movementState.SetDirection(_currentDirection);
        _climbState.SetDirection(_currentDirection);
    }
    public void SetAim(Vector2 aim)
    {
        _aimDirection = aim;
        _aimingState.SetDirection(_aimDirection);
    }
    public void OnShoot()
    {
        if (_sm.CurrentState is ZhmyhAimingState)
        {
            var arrow = Instantiate(_arrow, _bow.transform.position, Quaternion.identity).GetComponent<Arrow>();
            arrow.Init(_aimDirection);
        }
    }
    public void OnClimb()
    {
        if (_climbState.ClimbController.IsClimb()) _isClimbing = !_isClimbing;
    }
    public void Run()
    {
        _sm.Update();
    }
    public void SetAimReady()
    {
        _isAiming = !_isAiming;
    }
}
