using UnityEngine;
using System.Collections;

public class DirectionalDasher : IDasher
{
    private Transform _transform;

    private float _distance;
    private float _duration;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _elapsedTime;

    public DirectionalDasher(Transform transform) :
        this (transform,5f,0.4f)
    { }
    public DirectionalDasher(Transform transform, float distance, float duration)
    {
        _transform = transform;
        _distance = distance;
        _duration = duration;
    }
    public void SetDirection(Vector3 direction)
    {
        _startPosition = _transform.position;
        _targetPosition = _startPosition + direction * _distance;
        _elapsedTime = 0;
    }
    public float Dash()
    {
        var t = Mathf.Clamp01(_elapsedTime / _duration);

        _transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);

        _elapsedTime += Time.deltaTime;

        return t;
    }
}