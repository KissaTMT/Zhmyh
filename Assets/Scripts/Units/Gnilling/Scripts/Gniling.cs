using R3;
using UnityEngine;

public class Gniling : Unit
{
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _speed;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;

    [SerializeField] private Transform _handle;
    [SerializeField] private Sword _sword;

    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Vector3 _movementDirection;
    private Vector3 _lookDirection;

    private bool _isCast;

    private GnilingIdleState _idleState;
    private GnilingMovementState _movementState;
    private GnilingCastState _castState;

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

        _sword.Init(this, _handle);
        health = new Health(_maxHealth);
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
    public void Cast()
    {
        if (_sm.CurrentState is GnilingCastState) return;
        _isCast = true;
    }

    public void SetLookDirection(Vector2 input)
    {
        _lookDirection = input;
    }

    public void SetCastDirection(Vector3 input)
    {
        _lookDirection = input;
        _castState.SetDirection(input);
    }
    private void SetupStateMachine()
    {
        _sm = new StateMachine();

        _idleState = new GnilingIdleState(_shifter, _shiftAnimator);
        _movementState = new GnilingMovementState(transform, _shiftAnimator, _shifter, _speed);
        _castState = new GnilingCastState(_shifter, _sword, _handle);

        _sm.AddTransition(_idleState, _movementState,IdleToMovement);
        _sm.AddTransition(_movementState, _idleState, MovementToIdle);
        _sm.AddTransition(_idleState, _castState, ToCast, Empty);
        _sm.AddTransition(_movementState, _castState, ToCast);
        _sm.AddTransition(_castState, _movementState, CastToMovement);
        _sm.AddTransition(_castState, _idleState, CastToIdle);


        bool IdleToMovement() => _movementDirection.sqrMagnitude > 0.01f;
        bool MovementToIdle() => !IdleToMovement();
        bool ToCast() => _isCast;
        bool CastToMovement() => !_castState.IsCast && IdleToMovement();
        bool CastToIdle() => !_castState.IsCast && !IdleToMovement();
        void Empty() { };

        _sm.SetState(_idleState);
    }
}
