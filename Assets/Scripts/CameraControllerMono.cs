using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraControllerMono : MonoBehaviour
{
    private const float SENSITIVITY = 10;
    private const float ROTATION_DURATION = 0.3f;

    public Transform Transform => _cinemachineCamera.transform;
    private CinemachineCamera _cinemachineCamera;
    private CinemachineFollow _cinemachineFollow;
    private Zhmyh _unit;
    private IInput _input;

    private Vector3 _mainOffset;
    private bool _locked;
    private Vector2 _delta;
    private Coroutine _rotateRoutine;

    [Inject]
    public void Construct(IInput input, PlayerUnitBrian player)
    {
        _unit = player.Unit as Zhmyh;
        _input = input;
        _input.InitAiming += SetAimming;
    }

    private void Awake()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _cinemachineFollow = GetComponent<CinemachineFollow>();
        _cinemachineCamera.Follow = _unit.Transform;
        _mainOffset = _cinemachineFollow.FollowOffset;
    }

    private void OnDisable()
    {
        _input.InitAiming -= SetAimming;
    }

    private void SetAimming(bool aim)
    {
        if (aim)
        {
            _locked = true;
            if (_rotateRoutine != null) StopCoroutine(_rotateRoutine);
            _rotateRoutine = StartCoroutine(RotateToRoutine());
        }
        else
        {
            if (_rotateRoutine != null) StopCoroutine(_rotateRoutine);
            _locked = false;
        }
    }

    private void Rotate(float delta)
    {
        _cinemachineCamera.transform.RotateAround(_unit.Transform.position, Vector3.up, delta * SENSITIVITY * Time.deltaTime);
        UpdateFollowOffset();
    }

    private void UpdateFollowOffset()
    {
        var angle = _cinemachineCamera.transform.eulerAngles.y * Mathf.Deg2Rad;
        var cos = Mathf.Cos(angle);
        var sin = Mathf.Sin(angle);
        _cinemachineFollow.FollowOffset = new Vector3(
            cos * _mainOffset.x + sin * _mainOffset.z,
            _mainOffset.y,
            -sin * _mainOffset.x + cos * _mainOffset.z);
    }

    private IEnumerator RotateToRoutine()
    {
        _locked = true;

        var cameraForward = _cinemachineCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        var unitForward = _unit.NotZeroMovementDirection;
        unitForward.y = 0;
        unitForward.Normalize();

        var dot = Vector3.Dot(cameraForward, unitForward);

        dot = Mathf.Clamp(dot, -1f, 1f);

        var angleBetween = Mathf.Acos(dot) * Mathf.Rad2Deg;

        var cross = Vector3.Cross(cameraForward, unitForward);
        var direction = Mathf.Sign(cross.y);

        var targetAngle = _cinemachineCamera.transform.eulerAngles.y + angleBetween * direction;

        var startAngle = _cinemachineCamera.transform.eulerAngles.y;
        var elapsedTime = 0f;

        while (elapsedTime < ROTATION_DURATION)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / ROTATION_DURATION;

            var currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);

            _cinemachineCamera.transform.rotation = Quaternion.Euler(
                _cinemachineCamera.transform.eulerAngles.x,
                currentAngle,
                _cinemachineCamera.transform.eulerAngles.z);

            UpdateFollowOffset();
            yield return null;
        }
        _cinemachineCamera.transform.rotation = Quaternion.Euler(30f, targetAngle, 0f);
        UpdateFollowOffset();

        _locked = false;
    }

    private void LateUpdate()
    {
        _delta = _input.GetAiming();
        if (_locked || _delta == Vector2.zero) return;
        Rotate(_delta.x);
    }
}