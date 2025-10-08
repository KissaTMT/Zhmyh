using UnityEngine;
using Zenject;

public class Gniling : Unit
{
    [SerializeField] private float _speed;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;
    private Transform _target;

    private IMover _movement;
    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Vector2 _currentDirection;
    private Vector2 _facingDirection;

    [Inject]
    public void Construct(PlayerUnitBrian player)
    {
        _target = player.Transform;
    }

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
        transform = GetComponent<Transform>();
        _movement = new DirectionalNotPhysicalMover(transform, _speed);
        _shifter = new Shifter(transform.GetChild(0), _shiftConfigs).SetPrimeShift();
        _shiftAnimator = new ShiftAnimator(_shifter,_shiftAnimations);

        _shiftAnimator.SetAnimation("idle");
        return this;
    }
    public void SetDirection(Vector2 input)
    {
        _currentDirection = input;
        _facingDirection = input;
    }
    public void Move()
    {
        _shiftAnimator.SetAnimation("walk");
        _movement.Move(new Vector3(_currentDirection.x,0,_currentDirection.y));
        _shifter.Shift(_facingDirection);
    }
    private void Awake()
    {
        Init();
    }
    private void Update()
    {
        Tick();
    }

    public override void Tick()
    {
        var delta = new Vector2(_target.position.x - transform.position.x, _target.position.z - transform.position.z);
        delta = Vector2.zero;
        if (delta.magnitude > 25) delta.Normalize();
        else delta = Vector2.zero;
        SetDirection(delta);
        _shiftAnimator.Tick();
        _shiftAnimator.SetDirection(_currentDirection);
        if (delta != Vector2.zero) Move();
        else _shiftAnimator.SetAnimation("idle");
    }
}
