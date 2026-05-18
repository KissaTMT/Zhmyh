using UnityEngine;
using UnityEngine.UIElements;

public class DirectionalCCDasher : IDasher
{
    private CharacterController _characterController;
    private AnimationCurve _animationCurve;
    private float _distance;
    private float _duration;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _elapsedTime;

    public DirectionalCCDasher(CharacterController characterController) :
    this(characterController, 5f, 0.4f)
    { }
    public DirectionalCCDasher(CharacterController characterController, float distance, float duration)
    {
        _characterController = characterController;
        _distance = distance;
        _duration = duration;
    }
    public DirectionalCCDasher(CharacterController characterController, AnimationCurve curve, float distance, float duration)
    {
        _characterController = characterController;
        _animationCurve = curve;
        _distance = distance;
        _duration = duration;
    }
    public float Dash()
    {
        var t = Mathf.Clamp01(_elapsedTime / _duration);

        var horizontalTarget = Vector3.Lerp(_startPosition, _targetPosition, _animationCurve.Evaluate(t));

        var currentPos = _characterController.transform.position;

        var delta = new Vector3(horizontalTarget.x - currentPos.x, 0, horizontalTarget.z - currentPos.z);

        _characterController.Move(delta);

        _elapsedTime += Time.deltaTime;

        return t;
    }

    public void SetDirection(Vector3 direction)
    {
        _startPosition = _characterController.transform.position;
        _targetPosition = _startPosition + direction * _distance;
        _elapsedTime = 0;
    }
}