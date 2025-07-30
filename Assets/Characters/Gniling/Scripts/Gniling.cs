using UnityEngine;
using Zenject;

public class Gniling : Unit
{
    [SerializeField] private float _speed;
    [SerializeField] private ShiftConfig[] _shiftConfigs;
    [SerializeField] private ShiftAnimation[] _shiftAnimations;
    private Transform _transform;
    private Transform _target;

    private IMovementController _movement;
    private Shifter _shifter;
    private ShiftAnimator _shiftAnimator;
    private StateMachine _sm;

    private Vector2 _currentDirection;
    private Vector2 _facingDirection;

    [Inject]
    public void Construct(Player player)
    {
        _target = player.Transform;
    }

    public Gniling Init()
    {
        for (var i = 0; i < _shiftConfigs.Length; i++)
        {
            _shiftConfigs[i].Deserialize();
        }
        for (var i = 0; i < _shiftAnimations.Length; i++)
        {
            _shiftAnimations[i].Deserialize();
        }
        _transform = GetComponent<Transform>();
        _movement = new NPhMovementController(_transform, _speed);
        _shifter = new Shifter(_transform.GetChild(0), _shiftConfigs).SetPrimeShift();
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
        _movement.Move(_currentDirection);
        _shifter.Shift(_facingDirection);
    }
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    protected override void Update()
    {
        base.Update();
        var delta = Vector2.zero;
        if (delta.magnitude > 25) delta.Normalize();
        else delta = Vector2.zero;
        SetDirection(delta);
        _shiftAnimator.Update();
        _shiftAnimator.SetDirection(_currentDirection);
        if (delta != Vector2.zero) Move();
        else _shiftAnimator.SetAnimation("idle");
    }
}
