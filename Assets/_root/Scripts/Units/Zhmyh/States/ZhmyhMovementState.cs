using UnityEngine;

public class ZhmyhMovementState : State
{
    private IMover _mover;
    private Shifter _shifter;

    private CharacterController _characterController;
    private ShiftAnimator _shiftAnimator;
    private Vector3 _direction;
    public ZhmyhMovementState(Transform transform, ShiftAnimator shiftAnimator, Shifter shifter, float speed)
    {
        _characterController = transform.GetComponent<CharacterController>();
        _mover = new DirectionalCCMover(_characterController, speed);
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
    }
    public void Move()
    {
        _mover.Move(_direction);
        _characterController.Move(new Vector3(0,Physics.gravity.y * 0.5f,0));
    }
    public void Shift()
    {
        var rotation = _shifter.Root.localEulerAngles.y * Mathf.Deg2Rad;

        var cos = Mathf.Cos(rotation);
        var sin = Mathf.Sin(rotation);

        var x = _direction.x * cos - _direction.z * sin;
        var y = _direction.x * sin + _direction.z * cos;

        _shifter.Shift(new Vector2(x,y));
    }
}
