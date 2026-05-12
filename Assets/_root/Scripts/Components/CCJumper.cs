using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class CCJumper
{
    private CharacterController _characterController;
    private AnimationCurve _animationCurve;
    private Transform _transform;
    private Transform _groundChecker;
    private LayerMask _layerMask;
    private float _height;
    private float _duration;
    private float _elapsedTime;

    private float _startHeight;
    public CCJumper(Transform transform, AnimationCurve curve, float height, float duration, Transform groundChecker, LayerMask layerMask)
    {
        _characterController = transform.GetComponent<CharacterController>();
        _transform = transform;
        _animationCurve = curve;
        _groundChecker = groundChecker;
        _layerMask = layerMask;
        _height = height;
        _duration = duration;
    }
    public bool CanJumpExecute()
    {
        return _characterController.isGrounded;
    }
    public float Jump()
    {
        var t = Mathf.Clamp01(_elapsedTime / _duration);

        var deltaY = _startHeight + _animationCurve.Evaluate(t) * _height - _transform.position.y;

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