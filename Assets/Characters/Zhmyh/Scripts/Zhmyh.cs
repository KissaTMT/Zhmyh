using UnityEngine;

public class Zhmyh : Unit
{
    public Transform Transform => _transform;
    
    [SerializeField] private float _movementSpeed;
    [SerializeField] private float _climbSpeed;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;

    [SerializeField] private Bow _bow;
    [SerializeField] private Transform _body;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftHand;

    private Transform _transform;

    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Vector2 _currentDirection = Vector2.zero;
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
        for (var i = 0; i < _shiftConfigs.Length; i++)
        {
            _shiftConfigs[i].Deserialize();
        }
        for (var i = 0; i < _shiftAnimations.Length; i++)
        {
            _shiftAnimations[i].Deserialize();
        }

        _shifter = new Shifter(_transform.GetChild(0), _shiftConfigs).SetPrimeShift();
        _shiftAnimator = new ShiftAnimator(_shifter, _shiftAnimations);
        _shiftAnimator.SetDirection(new Vector2(1, -1));

        _sm = new StateMachine();

        _idleState = new ZhmyhIdleState(_shiftAnimator);
        _movementState = new ZhmyhMovementState(_transform, _shiftAnimator, _shifter, _movementSpeed);
        _climbState = new ZhmyhClimbState(_body, _shiftAnimator, _shifter, _climbSpeed);
        _aimingState = new ZhmyhAimingState(this, transform, _rightHand, _leftHand, _bow, _shifter);

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
        _shiftAnimator.SetDirection(_currentDirection);
    }
    public void SetAim(Vector2 input)
    {
        _aimDirection = input;
        _aimingState.SetDirection(_aimDirection);
        //Debug.Log(_aimDirection);
    }
    public void OnClimb()
    {
        if (_climbState.ClimbController.IsClimb()) _isClimbing = !_isClimbing;
    }
    public void Tick()
    {
        _sm.Update();
        _shiftAnimator.Update();
    }
    public void SetAimReady(bool isAiming)
    {
        _isAiming = isAiming;
    }
    public void Shoot()
    {
        _aimingState.Shoot();
    }
    private void OnDisable()
    {
        _shiftAnimator.Dispose();
    }
}
