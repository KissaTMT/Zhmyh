using UnityEngine;

public class ZhmyhDashState : State
{
    public float Progress => Mathf.Clamp01(_progress);
    private IDasher _dasher;

    private Transform _transform;
    private CharacterController _characterController;
    private Transform _root;
    private AnimationCurve _curve;
    private Vector3 _direction;
    private Vector2 _lookDirection;

    private Vector3 _currentDirection;
    private Vector2 _currentLookDirection;
    private Vector2 _startLocalEulerAnglesXZ;
    private Vector3 _startPosition;
    private float _distance;
    private float _duration;
    private float _progress;
    public ZhmyhDashState(Transform transform, AnimationCurve curve, float distance, float duration)
    {
        _transform = transform;
        _characterController = transform.GetComponent<CharacterController>();
        _curve = curve;
        _distance = distance;
        _duration = duration;
        _dasher = new DirectionalCCDasher(_transform.GetComponent<CharacterController>(), _distance, _duration);
        _root = _transform.GetChild(0);
        _startLocalEulerAnglesXZ = new Vector2(_root.localEulerAngles.x, _root.localEulerAngles.z);
    }
    public override void OnEnter()
    {
        _currentDirection = _direction.normalized;
        _currentLookDirection = _lookDirection.normalized;
        _startPosition = _transform.position;
        _dasher.SetDirection(_currentDirection);
    }
    public override void OnTick()
    {
        Dash();
    }
    public override void OnExit()
    {
        _root.localEulerAngles = new Vector3(_startLocalEulerAnglesXZ.x, _root.localEulerAngles.y, _startLocalEulerAnglesXZ.y);
        _transform.position = new Vector3(_transform.position.x, _startPosition.y, _transform.position.z);
    }
    public void Dash()
    {
        _progress = _dasher.Dash();
        _root.localEulerAngles =
            new Vector3(_root.localEulerAngles.x, _root.localEulerAngles.y,
            Mathf.Lerp(_startLocalEulerAnglesXZ.y, _startLocalEulerAnglesXZ.y - 320 * Mathf.Sign(_currentLookDirection.x), _progress));
        _characterController.Move(
            new Vector3(0, Mathf.Lerp(_startPosition.y, _startPosition.y + 2 * _curve.Evaluate(_progress), _progress) - _transform.position.y, 0));
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