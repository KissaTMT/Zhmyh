using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CCJumper
{
    private CharacterController _characterController;
    private AnimationCurve _animationCurve;
    private Transform _groundChecker;
    private LayerMask _layerMask;
    private float _height;
    private float _duration;
    private float _elapsedTime;

    private float _startHeight;
    public CCJumper(CharacterController controller, AnimationCurve curve, float height, float duration, Transform groundChecker, LayerMask layerMask)
    {
        _characterController = controller;
        _animationCurve = curve;
        _groundChecker = groundChecker;
        _layerMask = layerMask;
        _height = height;
        _duration = duration;
    }
    public bool CanJumpExecute()
    {
        return _characterController.isGrounded;
        return Physics.SphereCast(_groundChecker.position, 1, -_groundChecker.up, out var hit, 4, _layerMask.value);
    }
    public float Jump()
    {
        var t = Mathf.Clamp01(_elapsedTime / _duration);

        var deltaY = _animationCurve.Evaluate(t) * _height - _startHeight;

        _characterController.Move(new Vector3(0, deltaY, 0));

        _elapsedTime += Time.deltaTime;

        return t;
    }
    public void PerfomJump()
    {
        _startHeight = _characterController.transform.position.y;
        _elapsedTime = 0;
    }
}