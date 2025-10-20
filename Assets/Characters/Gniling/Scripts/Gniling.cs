using UnityEngine;

public class Gniling : Unit
{
    [SerializeField] private float _speed;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;

    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Vector3 _movementDirection;
    private Vector3 _lookDirection;

    private GnilingIdleState _idleState;
    private GnilingMovementState _movementState;

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
        _shifter = new Shifter(transform.GetChild(0), _shiftConfigs).SetPrimeShift();
        _shiftAnimator = new ShiftAnimator(_shifter,_shiftAnimations);

        _lookDirection = _shifter.CurrentDirection;


        SetupStateMachine();
    }
    public void SetMovementDirection(Vector3 input)
    {
        _movementDirection = input;
        _idleState.SetDirection(_movementDirection);
        _movementState.SetDirection(_movementDirection);
    }

    public override void Tick()
    {
        _sm.Tick();
        _shiftAnimator.Tick();
    }

    protected override void SetupProperties()
    {
        //throw new System.NotImplementedException();
    }

    public void SetLookDirection(Vector2 input)
    {
        _lookDirection = input;
    }

    public void SetShootDirection(Vector3 input)
    {
        _lookDirection = input;
    }
    private void SetupStateMachine()
    {
        _sm = new StateMachine();

        _idleState = new GnilingIdleState(_shifter, _shiftAnimator);
        _movementState = new GnilingMovementState(transform, _shiftAnimator, _shifter, _speed);

        _sm.AddTransition(_idleState, _movementState,IdleToMovement);
        _sm.AddTransition(_movementState, _idleState, MovementToIdle);


        bool IdleToMovement() => _movementDirection.sqrMagnitude > 0.01f;
        bool MovementToIdle() => !IdleToMovement();
        _sm.SetState(_idleState);
    }
}
