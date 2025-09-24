using System;
using System.Collections;
using UnityEngine;

public class Zhmyh : Unit
{
    public State CurrentState => _sm.CurrentState;
    public Health Health => _health;
    public Vector2 LookDirection => _lookDirection;

    [SerializeField] private float _maxHealth;
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _climbSpeed;
    [SerializeField] private float _dashDistance;
    [SerializeField] private float _dashDuration;
    [SerializeField] private AnimationCurve _dashHeightCurve;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;

    [SerializeField] private Bow _bow;
    [SerializeField] private Transform _body;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftHand;

    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Health _health;

    private Vector3 _movementDirection = Vector3.zero;
    private Vector2 _lookDirection = Vector3.zero;

    private bool _isClimbing = false;
    private bool _isAiming = false;
    private bool _isDashing = false;

    private ZhmyhIdleState _idleState;
    private ZhmyhMovementState _movementState;
    private ZhmyhClimbState _climbState;
    private ZhmyhAimingState _aimingState;
    private ZhmyhDashState _dashState;
    public override Unit Init()
    {
        base.Init();
        for (var i = 0; i < _shiftConfigs.Length; i++)
        {
            _shiftConfigs[i].Deserialize();
        }
        for (var i = 0; i < _shiftAnimations.Length; i++)
        {
            _shiftAnimations[i].Deserialize();
        }

        _shifter = new Shifter(transform.GetChild(0), _shiftConfigs).SetPrimeShift();
        _shiftAnimator = new ShiftAnimator(_shifter, _shiftAnimations);
        _shiftAnimator.SetDirection(new Vector2(1, -1));

        _sm = new StateMachine();

        _idleState = new ZhmyhIdleState(_shiftAnimator);
        _movementState = new ZhmyhMovementState(transform, _shiftAnimator, _shifter, _movementSpeed);
        _climbState = new ZhmyhClimbState(_body, _shiftAnimator, _shifter, _climbSpeed);
        _aimingState = new ZhmyhAimingState(this, transform, _rightHand, _leftHand, _bow, _shifter);
        _dashState = new ZhmyhDashState(transform, _dashHeightCurve, _dashDistance,_dashDuration);

        _sm.AddTransition(_idleState, _movementState, () => _movementDirection.sqrMagnitude > 0.01f);
        _sm.AddTransition(_movementState, _idleState, () => _movementDirection.sqrMagnitude < 0.01f);
        _sm.AddAnyTransition(_climbState, () => _isClimbing == true);
        _sm.AddTransition(_climbState, _idleState, () => _isClimbing == false);
        _sm.AddTransition(_idleState, _aimingState, () => _isAiming == true);
        _sm.AddTransition(_movementState, _aimingState, () => _isAiming == true, _movementState.Move);
        _sm.AddTransition(_aimingState, _movementState, () => _isAiming == false && _movementDirection.sqrMagnitude > 0.01f);
        _sm.AddTransition(_aimingState, _idleState, () => _isAiming == false && _movementDirection.sqrMagnitude < 0.01f);
        _sm.AddTransition(_movementState, _dashState, () => _isDashing == true);
        _sm.AddTransition(_dashState, _movementState, () => _dashState.Progress == 1 && _movementDirection.sqrMagnitude > 0.01f);
        _sm.AddTransition(_dashState, _idleState, () => _dashState.Progress == 1 && _movementDirection.sqrMagnitude < 0.01f);

        _sm.SetState(_idleState);

        _health = new Health(_maxHealth);
        _health.Current.OnChanged += HealthHandle;

        return this;
    }

    private void HealthHandle(float health)
    {
        Debug.Log($"{this} {health}");
        if (health == 0) Debug.Log($"{this} is fucking dead");
    }

    public override void Tick()
    {
        _sm.Tick();
        _shiftAnimator.Update();
        _dashState.SetLookDirection(_shifter.CurrentDirection);
    }
    public void SetMovementDirection(Vector3 input)
    {
        if(_movementDirection == input) return;
        _movementDirection = input;

        _movementState.SetDirection(_movementDirection);
        _climbState.SetDirection(_movementDirection);
        _dashState.SetDirection(_movementDirection);
        _shiftAnimator.SetDirection(_movementDirection);
    }
    public void SetLookDirection(Vector2 input)
    {
        if(_lookDirection == input) return;
        _lookDirection = input;
        _aimingState.SetLookDirection(_lookDirection);
    }
    public void Dash()
    {
        if (CurrentState is ZhmyhDashState dashState && dashState.Progress < 1f) return;
        _isDashing = true;
        StartCoroutine(ResetRoutine());
    }
    public void SetAim(bool isAiming)
    {
        _isAiming = isAiming;
    }
    public void Pull(bool isPull)
    {
        if (CurrentState is ZhmyhAimingState == false) return;
        _aimingState.SetPull(isPull);
        if(isPull == false) Release();
    }
    public void Release()
    {
        _aimingState.Release();
    }
    private void OnDisable()
    {
        _shiftAnimator.Dispose();
    }
    private IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(_dashDuration);
        _isDashing = false;
    }
}
