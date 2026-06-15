using Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZhmyhJumpState : State, IContributable<Vector3>
{
    public float Progress => _progress;
    public Jumper Jumper => _jumper;

    public Vector3 Contribute => _result;

    private Transform _transform;
    private Jumper _jumper;
    private Shifter _shifter;

    private Vector3 _direction;
    private float _speedInAir;


    private Vector3 _result;
    private float _progress;
    private MonoBehaviour _ctx;
    private Vector3 _scale;

    public ZhmyhJumpState(Transform transform, AnimationCurve curve, float speedInAir, float height, float duration, Shifter shifter, MonoBehaviour ctx)
    {
        _transform = transform;
        _jumper = new Jumper(curve, height, duration);
        _speedInAir = speedInAir;
        _shifter = shifter;
        _ctx = ctx;
        _scale = _transform.localScale;
    }

    public override void OnEnter()
    {
        _jumper.PerfomJump();
        _transform.localScale = _scale;
        _ctx.StartCoroutine(PopupRoutine(0.1f));
    }
    public override void OnExit()
    {
        _result = Vector3.zero;
        _ctx.StartCoroutine(PopupRoutine(0.2f));
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
    private IEnumerator PopupRoutine(float offset)
    {
        var start = _transform.localScale;
        var current = start;

        for (var i = 0f; i < 1f; i += Time.deltaTime * 8)
        {
            var scale = new Vector3(start.x, Mathf.Lerp(current.y, current.y - offset, i), start.z);
            _transform.localScale = scale;
            yield return null;
        }

        current = _transform.localScale;
        for (var i = 0f; i < 1f; i += Time.deltaTime * 8)
        {
            var scale = new Vector3(start.x, Mathf.Lerp(current.y, current.y + offset, i), start.z);
            _transform.localScale = scale;
            yield return null;
        }

        _transform.localScale = _scale;
    }
}
