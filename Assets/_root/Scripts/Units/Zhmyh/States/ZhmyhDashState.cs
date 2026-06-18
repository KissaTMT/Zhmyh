using Components;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ZhmyhDashState : State, IContributable<Vector3>
{
    public float Progress => Mathf.Clamp01(_progress);

    public Vector3 Contribute => _result;

    private Dasher _dasher;

    private Transform _transform;
    private Transform _root;
    private ShiftAnimator _shiftAnimator;
    private AnimationCurve _height;
    private AnimationCurve _dash;
    private Vector3 _direction;
    private Vector2 _lookDirection;

    private Vector3 _currentDirection;
    private Vector2 _currentLookDirection;
    private Vector2 _startLocalEulerAnglesXZ;
    private Vector3 _startPosition;
    private Vector3 _startLocalScale;
    private float _distance;
    private float _duration;
    private float _progress;

    private Vector3 _result;
    private MonoBehaviour _ctx;
    private Gravity _gravity;

    public ZhmyhDashState(Transform transform, AnimationCurve dash, AnimationCurve height, ShiftAnimator shiftAnimator,float distance, float duration, MonoBehaviour ctx, Gravity gravity)
    {
        _transform = transform;
        _dash = dash;
        _height = height;
        _distance = distance;
        _duration = duration;
        _shiftAnimator = shiftAnimator;
        _startLocalScale = _transform.localScale;
        _dasher = new Dasher(dash, _distance, _duration);
        _root = _transform.GetChild(0);
        _startLocalEulerAnglesXZ = new Vector2(_root.localEulerAngles.x, _root.localEulerAngles.z);

        _ctx = ctx;
        _gravity = gravity;
    }


    public override void OnEnter()
    {
        _gravity.Zero();
        _shiftAnimator.SetAnimation("idle");
        _currentDirection = _direction.normalized;
        _currentLookDirection = _lookDirection.normalized;
        _startPosition = _transform.position;
        _dasher.PerfomDash(_startPosition, _currentDirection);

        _ctx.StartCoroutine(Pop(0.2f));

    }
    public override void OnTick()
    {
        Dash();
    }
    public override void OnExit()
    {
        _root.localEulerAngles = new Vector3(_startLocalEulerAnglesXZ.x, _root.localEulerAngles.y, _startLocalEulerAnglesXZ.y);
        _result = Vector3.zero;
        _ctx.StartCoroutine(Pup());
    }
    public void Dash()
    {
        _dasher.Tick(Time.deltaTime);
        _progress = _dasher.Current;

        float deltaY = _startPosition.y + 0.0f * _height.Evaluate(_progress) - _transform.position.y;


        _result = _dasher.Contribute;

        //_root.localEulerAngles =
        //    new Vector3(_root.localEulerAngles.x, _root.localEulerAngles.y,
        //    Mathf.LerpUnclamped(_startLocalEulerAnglesXZ.y, _startLocalEulerAnglesXZ.y - 320 * Mathf.Sign(_currentLookDirection.x), _progress * 2));
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }
    public void SetLookDirection(Vector2 direction)
    {
        _lookDirection = direction;
    }
    private IEnumerator Pop(float offset)
    {
        var start = _startLocalScale;

        for (var i = 0f; i < 1f; i += Time.deltaTime * 4)
        {
            var scale = new Vector3(start.x, Mathf.Lerp(start.y, start.y - offset, i), start.z);
            _transform.localScale = scale;
            yield return null;
        }
        _transform.localScale = new Vector3(start.x, start.y - offset, start.z);
    }
    private IEnumerator Pup()
    {
        var start = _transform.localScale;
        var delta = _startLocalScale.y - start.y;

        for (var i = 0f; i < 1f; i += Time.deltaTime * 6)
        {
            var scale = new Vector3(start.x, Mathf.Lerp(start.y, start.y + delta, i), start.z);
            _transform.localScale = scale;
            yield return null;
        }
        _transform.localScale = _startLocalScale;
    }
}