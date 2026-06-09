using Components;
using Inversion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverted : MonoBehaviour
{
    [SerializeField] private Vector3 _impulseForce = new Vector3(-0.1f, 0, 0);
    [SerializeField] private float _atenuationTime = 1;
    [SerializeField] private bool _isPhysical = false;

    private Transform _transform;
    private Rigidbody _rigidbody;

    private Gravity _gravity;
    private Impulse _impulse;
    private IGroundChecker _groundChecker;

    private bool _isGrounded;


    private List<Vector3> _buffer;
    private Coroutine _coroutine;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _gravity = new Gravity();
        _impulse = new Impulse();
        _gravity.Zero();
        _groundChecker = new GroundSphereOverlapChecker(LayerMask.GetMask("Default", "Ground"), _transform);

        _buffer = Predict(_transform.position);

        var collider = GetComponent<Collider>();

        _impulse.Init(_impulseForce, _atenuationTime);

        _gravity.Zero();
        _gravity.Enable();

        StartCoroutine(ImpulseRoutine());
    }

    private IEnumerator ImpulseRoutine()
    {
        yield return new WaitForSeconds(1);
        _impulse.Init(_impulseForce, _atenuationTime);
        yield return new WaitUntil(() => _impulse.Contribute == Vector3.zero);
        yield return new WaitForSeconds(0.25f);
        _coroutine = StartCoroutine(InvertRoutine());
    }
    private void Update()
    {
        if (_coroutine != null) return;


        _isGrounded = CheckGround(_transform.position, out var offset);

        var result = CalculateMovement(Time.deltaTime);

        var position = new Vector3(_rigidbody.position.x, offset, _rigidbody.position.z);

        _rigidbody.MovePosition(position + result);
        var axis = -new Vector3(_impulse.Contribute.z, _impulse.Contribute.y, _impulse.Contribute.x);
        var angle = axis.magnitude * 80 * Time.deltaTime;
        _rigidbody.MoveRotation(Quaternion.AngleAxis(angle, axis.normalized) * _rigidbody.rotation);
    }

    private IEnumerator InvertRoutine()
    {
        if (_buffer.Count < 2) yield break;


        for (int i = _buffer.Count - 1; i > 0; i--)
        {
            var target = _buffer[i];

            var delta = (target - _rigidbody.position);

            while (Vector3.Distance(_rigidbody.position, target) > 0.1f)
            {
                _rigidbody.MovePosition(Vector3.Lerp(_rigidbody.position, _rigidbody.position + delta, Time.deltaTime));
                yield return null;
            }
            _rigidbody.position = target;
        }
    }

    private List<Vector3> Predict(Vector3 start)
    {
        var result = new List<Vector3>();

        var position = start;

        _impulse.Init(_impulseForce, _atenuationTime);

        var delta = 0.05f;

        result.Add(start);

        while (true)
        {
            _isGrounded = CheckGround(position, out var offset);
            position += CalculateMovement(delta);

            if(_isGrounded) position.y = offset;

            if (_impulse.Contribute == Vector3.zero) break;

            result.Add(position);
        }
        
        return result;
    }
    private Vector3 CalculateMovement(float deltaTime)
    {
        
        _impulse.Tick(deltaTime);
        _gravity.Tick(deltaTime);
        if (_isGrounded) _gravity.Disable();
        else
        {
            if (_gravity.Enabled == false) _gravity.Zero();
            _gravity.Enable();
        }
        var gravity = _gravity.Contribute;
        var impulse = _impulse.Contribute * deltaTime;
        var result = gravity + impulse;

        if (_isGrounded)
        {
            result -= impulse * 0.25f;
        }
        return result;
    }
    private bool CheckGround(Vector3 point, out float pointY)
    {
        float radius = 0.04f;
        var result = Physics.SphereCast(point + Vector3.down * 0.5f, radius, Vector3.down, out var hit, 1f, LayerMask.GetMask("Default", "Ground"));
        
        pointY = result ? hit.point.y + radius : point.y;

        return result;
    }
}


