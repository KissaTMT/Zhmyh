using Components;
using NUnit.Framework;
using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zhmyh : Unit
{
    public State CurrentState => _sm.CurrentState;
    public Transform Root => _root;
    public Bow Bow => _bow;
    public Timeflow Timeflow;
    public Vector3 NotZeroMovementDirection { get; private set; }

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _climbSpeed;
    [SerializeField] private float _dashDistance;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _jumpDuration;
    [SerializeField] private AnimationCurve _dashHeightCurve;
    [SerializeField] private AnimationCurve _dashCurve;
    [SerializeField] private AnimationCurve _jumpCurve;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;

    [SerializeField] private Bow _bow;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftHand;


    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Transform _root;
    private CharacterController _characterController;

    private Vector3 _movementDirection = Vector3.zero;
    private Vector3 _lookDirection = Vector3.zero;

    private bool _isClimbing = false;
    private bool _isAiming = false;
    private bool _isDashing = false;
    private bool _isJumping = false;
    private bool _isGrounded = false;

    private ZhmyhIdleState _idleState;
    private ZhmyhMovementState _movementState;
    private ZhmyhClimbState _climbState;
    private ZhmyhAimingState _aimingState;
    private ZhmyhDashState _dashState;
    private ZhmyhJumpState _jumpState;

    private IMovementHandler _movementHandler;
    private Gravity _gravity;
    private IGroundChecker _groundChecker;
    private float _coyoteTime;
    protected override void OnInit()
    {
        for (var i = 0; i < _shiftConfigs.Length; i++)
        {
            _shiftConfigs[i].Deserialize();
        }
        for (var i = 0; i < _shiftAnimations.Length; i++)
        {
            _shiftAnimations[i].Deserialize();
        }

        _root = transform.GetChild(0);

        _shifter = new Shifter(_root, _shiftConfigs).SetPrimeShift();
        _shiftAnimator = new ShiftAnimator(_shifter, _shiftAnimations);
        _characterController = GetComponent<CharacterController>();

        health = new Health(_maxHealth);

        NotZeroMovementDirection = new Vector3(_shifter.CurrentDirection.x, 0, _shifter.CurrentDirection.y);


        _movementHandler = new CCMovementHandler(_characterController);
        _gravity = new Gravity();
        _groundChecker = new GroundSphereOverlapChecker(LayerMask.GetMask("Default", "Ground"),transform);

        SetupStateMachine();
    }

    public override void Tick()
    {
        _isGrounded = CheckGround();

        if (_isGrounded)
        {
            _gravity.Disable();
            _coyoteTime = 0;
        }
        else
        {
            _coyoteTime += Time.deltaTime;
            if (_gravity.Enabled == false) _gravity.SetVelocity(4);
            _gravity.Enable();
        }
        _sm.Tick();
        _shiftAnimator.Tick();
        if (_gravity.Enabled) _gravity.Tick(Time.deltaTime);
        _movementHandler.Tick(Time.deltaTime);
    }
    public void SetMovementDirection(Vector3 input)
    {
        if (_movementDirection == input) return;
        _movementDirection = input;

        _idleState.SetDirection(_movementDirection);
        _movementState.SetDirection(_movementDirection);
        _climbState.SetDirection(new Vector2(_movementDirection.x, _movementDirection.z));
        _dashState.SetDirection(_movementDirection);
        _jumpState.SetDirection(_movementDirection);

        _dashState.SetLookDirection(_shifter.CurrentDirection);

        if (_movementDirection == Vector3.zero) return;

        NotZeroMovementDirection = _movementDirection;
    }
    public void SetLookDirection(Vector3 input)
    {
        if (_lookDirection == input) return;
        _lookDirection = input;
        _aimingState.SetLookDirection(input);

    }
    public void SetShootDirection(Vector3 input)
    {
        _aimingState.SetShootDirection(input);
    }
    public void Dash()
    {
        if (CurrentState is ZhmyhDashState dashState && dashState.Progress < 1f) return;

        
        _isDashing = true;
    }
    public void Jump()
    {
        if (_coyoteTime > 0.15f) return;

        _isJumping = true;
    }
    public void Climb()
    {
        if (_isClimbing) _isClimbing = false;
        else if (_climbState.Climber.IsClimb()) _isClimbing = true;
    }
    public void SetAim(bool isAiming)
    {
        _isAiming = isAiming;
    }
    public void Pull(bool isPull)
    {
        if (CurrentState is ZhmyhAimingState == false) SetAim(isPull);
        _aimingState.SetPull(isPull);
        if (!isPull) SetAim(false);
    }
    private bool CheckGround()
    {
        return _groundChecker.Check(transform.position + Vector3.down * 0.05f, 0.05f);
    }
    private void SetupStateMachine()
    {
        _sm = new StateMachine();

        _idleState = new ZhmyhIdleState(_shifter, _shiftAnimator);
        _movementState = new ZhmyhMovementState(transform, _shiftAnimator, _shifter, _movementSpeed);
        _climbState = new ZhmyhClimbState(transform, _shiftAnimator, _shifter, _climbSpeed);
        _aimingState = new ZhmyhAimingState(this, transform, _rightHand, _leftHand, _bow, _shifter);
        _dashState = new ZhmyhDashState(transform, _dashCurve, _dashHeightCurve, _shiftAnimator,_dashDistance, _dashDuration,this, _gravity);
        _jumpState = new ZhmyhJumpState(transform, _jumpCurve, _movementSpeed * 0.9f, _jumpHeight, _jumpDuration, _shifter, this, _gravity);

        _movementHandler.Add(_idleState);
        _movementHandler.Add(_movementState);
        _movementHandler.Add(_dashState);
        _movementHandler.Add(_jumpState);
        _movementHandler.Add(_gravity);

        _sm.AddTransition(_idleState, _movementState, IdleToMovement);
        _sm.AddTransition(_movementState, _idleState, MovementToIdle);
        _sm.AddTransition(_idleState, _jumpState, ToJump);
        _sm.AddTransition(_movementState, _jumpState, ToJump);
        _sm.AddAnyTransition(_climbState, ToClimb);
        _sm.AddTransition(_jumpState, _idleState, JumpToIdle);
        _sm.AddTransition(_jumpState, _movementState, JumpToMovement);
        _sm.AddTransition(_jumpState, _dashState, JumpToDash);
        _sm.AddTransition(_climbState, _idleState, ClimbToIdle);
        _sm.AddTransition(_idleState, _aimingState, ToAim, Empty);
        _sm.AddTransition(_movementState, _aimingState, ToAim, _movementState.Move);
        _sm.AddTransition(_aimingState, _movementState, AimToMovement);
        _sm.AddTransition(_aimingState, _idleState, AimToIdle);
        _sm.AddTransition(_movementState, _dashState, MovementToDash);
        _sm.AddTransition(_dashState, _movementState, DashToMovement);
        _sm.AddTransition(_dashState, _idleState, DashToIdle);

        _sm.SetState(_idleState);

        bool IdleToMovement() => _movementDirection.sqrMagnitude > 0.01f;
        bool MovementToIdle() => !IdleToMovement();
        bool ToClimb() => _isClimbing == true;
        bool ClimbToIdle() => !ToClimb();
        bool ToAim() => _isAiming == true;
        bool AimToIdle() => !ToAim() && MovementToIdle();
        bool AimToMovement() => !ToAim() && IdleToMovement();
        bool MovementToDash() => ToDash();
        bool DashTo() => _dashState.Progress == 1;
        bool DashToMovement() => DashTo() && IdleToMovement();
        bool DashToIdle() => DashTo() && !IdleToMovement();
        bool ToJump() => _isJumping;
        bool ToDash() => IdleToMovement() && _isDashing == true;
        bool JumpTo() => _jumpState.Progress > 0.2f && _isGrounded;
        bool JumpToIdle() => JumpTo() && !IdleToMovement();
        bool JumpToMovement() => JumpTo() && IdleToMovement();
        bool JumpToDash() => IdleToMovement() && ToDash();
        void Empty() { }
    }
    private void OnDisable()
    {
        _shiftAnimator.Dispose();
    }
    private void LateUpdate()
    {
        _isDashing = false;
        _isJumping = false;
    }
}
