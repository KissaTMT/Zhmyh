using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class OrbitalCameraControllerMono : MonoBehaviour
{
    private const float SENSITIVITY = 10f;
    private const float ROTATION_DURATION = 0.3f;

    public Transform Transform => _transform;

    private CinemachineCamera _cinemachineCamera;
    private CinemachineOrbitalFollow _orbitalFollow;
    private CinemachineRotationComposer _rotationComposer;
    private CinemachineCameraOffset _cameraOffset;

    private Zhmyh _unit;
    private IInput _input;
    private Transform _transform;

    private bool _locked;
    private float _sensitivity;
    private Vector2 _delta;
    private Vector3 _rotationOffset;
    private Coroutine _rotateRoutine;

    [Inject]
    public void Construct(IInput input, PlayerZhmyhBrian player)
    {
        _unit = player.Unit as Zhmyh;
        _input = input;
        _input.CameraReset += SetCameraDirectionToLookDirectionOfUnit;
        _input.Pulling += SetCameraToAimMode;
    }
    private void Awake()
    {
        _cinemachineCamera = GetComponent<CinemachineCamera>();
        _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        _rotationComposer = GetComponent<CinemachineRotationComposer>();
        _cameraOffset = GetComponent<CinemachineCameraOffset>();

        _transform = GetComponent<Transform>();

        _cinemachineCamera.Follow = _unit.Transform;
        _cinemachineCamera.LookAt = _unit.Transform;
        _rotationOffset = _rotationComposer.TargetOffset;

        _sensitivity = SENSITIVITY;
    }

    private void OnDisable()
    {
        _input.CameraReset -= SetCameraDirectionToLookDirectionOfUnit;
        _input.Pulling -= SetCameraToAimMode;
        _rotationComposer.TargetOffset = _rotationOffset;
    }
    private void LateUpdate()
    {
        _delta = _input.GetLook();
        if (_locked || _delta == Vector2.zero) return;

        ApplyInput(_delta);
    }
    private void SetCameraDirectionToLookDirectionOfUnit()
    {
        if (_rotateRoutine == null) _rotateRoutine = StartCoroutine(SetCameraDirectionToLookDirectionOfUnitRoutine());
    }
    private void SetCameraToAimMode(bool aim)
    {
        StartCoroutine(SetCameraAimOffset(aim ? new Vector3(0, 4/20f, 64/20f) : Vector3.zero));
        _sensitivity = aim ? SENSITIVITY / 2 : SENSITIVITY;
    }

    private void ApplyInput(Vector2 delta)
    {
        var horizontal = _orbitalFollow.HorizontalAxis.Value + delta.x * _sensitivity * Time.deltaTime;
        _orbitalFollow.HorizontalAxis.Value = _orbitalFollow.HorizontalAxis.ClampValue(horizontal);

        var vertical = _orbitalFollow.VerticalAxis.Value - delta.y * _sensitivity * Time.deltaTime;
        _orbitalFollow.VerticalAxis.Value = _orbitalFollow.VerticalAxis.ClampValue(vertical);

        UpdateRotationOffset();
    }
    private void UpdateRotationOffset()
    {
        var angle = _orbitalFollow.HorizontalAxis.Value * Mathf.Deg2Rad;
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);
        _rotationComposer.TargetOffset = new Vector3(
            cos * _rotationOffset.x + sin * _rotationOffset.z,
            _rotationOffset.y,
            -sin * _rotationOffset.x + cos * _rotationOffset.z);
    }
    private IEnumerator SetCameraAimOffset(Vector3 offset)
    {
        var startOffset = _cameraOffset.Offset;
        for(var t = 0f; t < 1f; t += 4 * Time.deltaTime)
        {
            _cameraOffset.Offset = Vector3.Lerp(startOffset, offset, t);
            yield return null;
        }
        _cameraOffset.Offset = offset;
    }
    private IEnumerator SetCameraDirectionToLookDirectionOfUnitRoutine()
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
            UpdateRotationOffset();
            yield return null;
        }

        _orbitalFollow.HorizontalAxis.Value = targetAngle;
        UpdateRotationOffset();
        _rotateRoutine = null;
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