using Components;
using UnityEngine;

public class ZhmyhJumpState : State, IContributable<Vector3>
{
    public float Progress => _progress;
    public Jumper Jumper => _jumper;

    public Vector3 Contribute => _result;

    private Jumper _jumper;
    private Shifter _shifter;

    private Vector3 _direction;
    private float _speedInAir;


    private Vector3 _result;
    private float _progress;

    public ZhmyhJumpState(Transform transform, AnimationCurve curve, float speedInAir,float height, float duration, Shifter shifter)
    {
        _jumper = new Jumper(curve, height, duration);
        _speedInAir = speedInAir;
        _shifter = shifter;
    }

    public override void OnEnter()
    {
        _jumper.PerfomJump();
    }
    public override void OnExit()
    {
        _result = Vector3.zero;
    }
    public override void OnTick()
    {
        Shift();

        _jumper.Tick(Time.deltaTime);
        _progress = _jumper.Current;
        _result = _jumper.Contribute + _direction * _speedInAir * Time.deltaTime;

    }
    public void SetDirection(Vector3 direction)
    {
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
