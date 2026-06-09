using Components;
using UnityEngine;

public class ZhmyhDashState : State, IContributable<Vector3>
{
    public float Progress => Mathf.Clamp01(_progress);

    public Vector3 Contribute => _result;

    private Dasher _dasher;

    private Transform _transform;
    private Transform _root;
    private AnimationCurve _height;
    private AnimationCurve _dash;
    private Vector3 _direction;
    private Vector2 _lookDirection;

    private Vector3 _currentDirection;
    private Vector2 _currentLookDirection;
    private Vector2 _startLocalEulerAnglesXZ;
    private Vector3 _startPosition;
    private float _distance;
    private float _duration;
    private float _progress;

    private Vector3 _result;

    public ZhmyhDashState(Transform transform, AnimationCurve dash, AnimationCurve height, float distance, float duration)
    {
        _transform = transform;
        _dash = dash;
        _height = height;
        _distance = distance;
        _duration = duration;
        _dasher = new Dasher(_distance, _duration);
        _root = _transform.GetChild(0);
        _startLocalEulerAnglesXZ = new Vector2(_root.localEulerAngles.x, _root.localEulerAngles.z);
    }


    public override void OnEnter()
    {
        _currentDirection = _direction.normalized;
        _currentLookDirection = _lookDirection.normalized;
        _startPosition = _transform.position;
        _dasher.PerfomDash(_startPosition, _currentDirection);

    }
    public override void OnTick()
    {
        Dash();
    }
    public override void OnExit()
    {
        _root.localEulerAngles = new Vector3(_startLocalEulerAnglesXZ.x, _root.localEulerAngles.y, _startLocalEulerAnglesXZ.y);
        _result = Vector3.zero;
    }
    public void Dash()
    {
        _dasher.Tick(Time.deltaTime);
        _progress = _dasher.Current;

        float deltaY = _startPosition.y + 1.5f * _height.Evaluate(_progress) - _transform.position.y;


        _result = _dasher.Contribute + Vector3.up * deltaY;

        _root.localEulerAngles =
            new Vector3(_root.localEulerAngles.x, _root.localEulerAngles.y,
            Mathf.LerpUnclamped(_startLocalEulerAnglesXZ.y, _startLocalEulerAnglesXZ.y - 320 * Mathf.Sign(_currentLookDirection.x), _progress * 2));
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }
    public void SetLookDirection(Vector2 direction)
    {
        _lookDirection = direction;
    }
}