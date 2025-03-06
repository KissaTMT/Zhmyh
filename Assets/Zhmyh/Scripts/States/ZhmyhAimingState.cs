using UnityEngine;

public class ZhmyhAimingState : DecorateState
{
    private Transform _transform;
    private Transform _bow;
    private Shifter _shifter;
    private Vector2 _aim;
    public ZhmyhAimingState(Transform transform, Transform bow, Shifter shifter)
    {
        _transform = transform;
        _bow = bow;
        _shifter = shifter;
    }
    public override void Enter()
    {
        base.Enter();
        Run = ReloadRun;
    }
    public override void ReloadRun()
    {
        Aiming();
        Shift();
    }
    public void SetDirection(Vector2 direction)
    {
        _aim = direction;
    }
    public void Aiming()
    {
        var delta = _aim - (Vector2)_transform.position;
        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        _bow.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    public void Shift() => _shifter.Shift(_aim);
    public override string ToString() => $"{base.ToString()} + {baseState}";
}
