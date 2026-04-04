using R3;
using System.Collections;
using UnityEngine;

public class Zhmyh : Unit
{
    public State CurrentState => _sm.CurrentState;
    public Transform Root => _root;
    public Bow Bow => _bow;
    public Vector3 NotZeroMovementDirection {  get; private set; }

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _climbSpeed;
    [SerializeField] private float _dashDistance;
    [SerializeField] private float _dashDuration;
    [SerializeField] private AnimationCurve _dashHeightCurve;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;

    [SerializeField] private Bow _bow;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftHand;

    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Transform _root;

    private Vector3 _movementDirection = Vector3.zero;
    private Vector3 _lookDirection = Vector3.zero;

    private bool _isClimbing = false;
    private bool _isAiming = false;
    private bool _isDashing = false;

    private ZhmyhIdleState _idleState;
    private ZhmyhMovementState _movementState;
    private ZhmyhClimbState _climbState;
    private ZhmyhAimingState _aimingState;
    private ZhmyhDashState _dashState;
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

        SetupStateMachine();

        health = new Health(_maxHealth);

        NotZeroMovementDirection = new Vector3(_shifter.CurrentDirection.x, 0, _shifter.CurrentDirection.y);
    }

    public override void Tick()
    {
        _sm.Tick();
        _shiftAnimator.Tick();
    }
    public void SetMovementDirection(Vector3 input)
    {
        if(_movementDirection == input) return;
        _movementDirection = input;

        _idleState.SetDirection(_movementDirection);
        _movementState.SetDirection(_movementDirection);
        _climbState.SetDirection(new Vector2(_movementDirection.x, _movementDirection.z));
        _dashState.SetDirection(_movementDirection);

        _dashState.SetLookDirection(_shifter.CurrentDirection);

        if (_movementDirection == Vector3.zero) return;

        NotZeroMovementDirection = _movementDirection;
    }
    public void SetLookDirection(Vector3 input)
    {
        if(_lookDirection == input) return;
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
    private void SetupStateMachine()
    {
        _sm = new StateMachine();

        _idleState = new ZhmyhIdleState(_shifter, _shiftAnimator);
        _movementState = new ZhmyhMovementState(transform, _shiftAnimator, _shifter, _movementSpeed);
        _climbState = new ZhmyhClimbState(transform, _shiftAnimator, _shifter, _climbSpeed);
        _aimingState = new ZhmyhAimingState(this, transform, _rightHand, _leftHand, _bow, _shifter);
        _dashState = new ZhmyhDashState(transform, _dashHeightCurve, _dashDistance, _dashDuration);

        _sm.AddTransition(_idleState, _movementState, IdleToMovement);
        _sm.AddTransition(_movementState, _idleState, MovementToIdle);
        _sm.AddAnyTransition(_climbState, ToClimb);
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
        bool MovementToDash() => _isDashing == true;
        bool DashTo() => _dashState.Progress == 1;
        bool DashToMovement() => DashTo() && IdleToMovement();
        bool DashToIdle() => DashTo() && !IdleToMovement();
        void Empty() { }
    }
    private void OnDisable()
    {
        _shiftAnimator.Dispose();
    }
    private void LateUpdate()
    {
        _isDashing = false;
    }
}
