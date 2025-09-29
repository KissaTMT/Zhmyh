using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class CameraControllerMono : MonoBehaviour
{
    private const float SENSITIVITY = 10f;
    private const float ROTATION_DURATION = 0.3f;

    public Transform Transform => _cinemachineCamera.transform;

    private CinemachineCamera _cinemachineCamera;
    private CinemachineOrbitalFollow _orbitalFollow;

    private Zhmyh _unit;
    private IInput _input;

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
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

        _cinemachineCamera.Follow = _unit.Transform;
        _cinemachineCamera.LookAt = _unit.Transform;
    }

    private void OnDisable()
    {
        _input.InitAiming -= SetAimming;
    }
    private void LateUpdate()
    {
        _delta = _input.GetAiming();
        if (_locked || _delta == Vector2.zero) return;

        ApplyInput(_delta);
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

    private void ApplyInput(Vector2 delta)
    {
        var horizontal = _orbitalFollow.HorizontalAxis.Value + delta.x * SENSITIVITY * Time.deltaTime;
        _orbitalFollow.HorizontalAxis.Value = _orbitalFollow.HorizontalAxis.ClampValue(horizontal);

        var vertical = _orbitalFollow.VerticalAxis.Value - delta.y * SENSITIVITY * Time.deltaTime;
        _orbitalFollow.VerticalAxis.Value = _orbitalFollow.VerticalAxis.ClampValue(vertical);
    }

    private IEnumerator RotateToRoutine()
    {
        _locked = true;

        var targetAngle = CalculateBehindAngle();
        var startAngle = _orbitalFollow.HorizontalAxis.Value;

        var elapsedTime = 0f;
        while (elapsedTime < ROTATION_DURATION)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / ROTATION_DURATION;
            var currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            _orbitalFollow.HorizontalAxis.Value = currentAngle;
            yield return null;
        }

        _orbitalFollow.HorizontalAxis.Value = targetAngle;
        _locked = false;
    }

    private float CalculateBehindAngle()
    {
        var cameraForward = _cinemachineCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        var unitForward = _unit.NotZeroMovementDirection;
        unitForward.y = 0;
        unitForward.Normalize();

        var dot = Mathf.Clamp(Vector3.Dot(cameraForward, unitForward), -1f, 1f);
        var angleBetween = Mathf.Acos(dot) * Mathf.Rad2Deg;
        var cross = Vector3.Cross(cameraForward, unitForward);
        var direction = Mathf.Sign(cross.y);

        return _orbitalFollow.HorizontalAxis.Value + angleBetween * direction;
    }
}
