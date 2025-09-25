using UnityEngine;

public class ZhmyhIdleState : State
{
    private ShiftAnimator _shiftAnimator;
    private Shifter _shifter;
    private Vector3 _direction;
    public ZhmyhIdleState(Shifter shifter,ShiftAnimator shiftAnimator)
    {
        _shifter = shifter;
        _shiftAnimator = shiftAnimator;
        _direction = new Vector3(1,0,-1);
    }
    public override void OnEnter()
    {
        _shiftAnimator.SetAnimation("idle");
    }
    public override void OnTick()
    {
        Shift();
    }
    public void SetDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        _direction = direction;
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
