using Components;
using UnityEngine;

public class GnilingMovementState : State
{
    private Mover _mover;
    private Shifter _shifter;

    private CharacterController _characterController;
    private ShiftAnimator _shiftAnimator;
    private Vector3 _direction;
    public GnilingMovementState(Transform transform, ShiftAnimator shiftAnimator, Shifter shifter, float speed)
    {
        _mover = new Mover(speed);
        _characterController = transform.GetComponent<CharacterController>();
        _shiftAnimator = shiftAnimator;
        _shifter = shifter;
    }
    public override void OnEnter()
    {
        _shiftAnimator.SetAnimation("walk");
    }
    public override void OnTick()
    {
        Move();
        Shift();
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
        _mover.SetDirection(direction);
    }
    public void Move()
    {
        _mover.Tick(Time.deltaTime);
        _characterController.Move(_mover.Contribute);
    }
    public void Shift()
    {
        var rotation = _shifter.Root.localEulerAngles.y * Mathf.Deg2Rad;

        var cos = Mathf.Cos(rotation);
        var sin = Mathf.Sin(rotation);

        var x = _direction.x * cos - _direction.z * sin;
        var y = _direction.x * sin + _direction.z * cos;

        _shifter.Shift(new Vector2(x, y));
    }
}
