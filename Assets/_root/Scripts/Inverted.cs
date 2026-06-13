using Components;
using Inversion;
using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Inverted : MonoBehaviour
{
    [SerializeField] private Vector3 _impulseForce = new Vector3(-4f, 0, 0);
    [SerializeField] private float _atenuationTime = 1;
    [SerializeField] private bool _isPhysical = false;

    private Transform _transform;
    private Rigidbody _rigidbody;

    private Gravity _gravity;
    private Impulse _impulse;
    private Collider _collider;
    private bool _isGrounded;
    private bool _isPreviousGrounded;

    private Dictionary<Vector3, Action> _commands;
    
    private Timeflow _timeflow;

    private List<ITickable> _contributables;
    private Vector3 _currentPosition;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _gravity = new Gravity();
        _impulse = new Impulse();
        _gravity.Zero();

        _contributables = new List<ITickable>
        {
            _gravity,
            _impulse
        };

        _timeflow = new Timeflow().SetRelativeFlow(new Timeflow());

        _isPreviousGrounded = _isGrounded;
    }
    private void Start()
    {
        StartCoroutine(ImpulseRoutine());
        _currentPosition = _rigidbody.position;
    }

    private IEnumerator ImpulseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            _gravity.Enable();
            _gravity.SetTimeflow(_timeflow.Relative);
            _gravity.Zero();
            _gravity.SetDirection(Vector3.down);

            _impulse.Enable();
            _impulse.SetTimeflow(_timeflow.Relative);
            _impulse.Apply(_impulseForce, _atenuationTime);

            yield return new WaitUntil(() => _impulse.Progress == 1);
            yield return new WaitForSeconds(0.25f);
            yield return StartCoroutine(InvertRoutine(_contributables.Cast<IContributable<Vector3>>().ToList(), new Vector3(-7, 6.5f, 23)));

            _timeflow.Inverse();
        }
    }
    private void FixedUpdate()
    {
        if (_timeflow.Relative == -1) return;

        _isGrounded = CheckGround(_currentPosition);

        var delta = CalculateMovement(Time.deltaTime);

        _rigidbody.MovePosition(_rigidbody.position + delta);

        _currentPosition = _rigidbody.position;

        ApplyRotation();

        _isPreviousGrounded = _isGrounded;
    }
    private void ApplyRotation()
    {
        var axis = -new Vector3(_impulse.Contribute.z, _impulse.Contribute.y, _impulse.Contribute.x);
        var angle = axis.magnitude * 3200 * Time.deltaTime;
        _rigidbody.MoveRotation(Quaternion.AngleAxis(angle, axis.normalized) * _rigidbody.rotation);
    }

    private IEnumerator InvertRoutine(List<IContributable<Vector3>> contributables, Vector3 to)
    {
        _commands = Predict(to, _contributables.Cast<IContributable<Vector3>>().ToList());
        
        _timeflow.Inverse();

        _impulse.SetTimeflow(_timeflow.Relative);
        _gravity.SetTimeflow(_timeflow.Relative);

        _impulse.Enable();


        _impulse.Apply(-_impulseForce, _atenuationTime);
        _gravity.SetDirection(-_gravity.Direction);
        _gravity.Zero();
        _gravity.Disable();

        while (true)
        {
            TickContributables(Time.deltaTime);

            _rigidbody.MovePosition(_rigidbody.position + _impulse.Contribute + _gravity.Contribute);

            if (CalculateApprocach(_transform.position, out var key))
            {
                _rigidbody.position = key;
                _commands[key].Invoke();
                _commands.Remove(key);
            }

            ApplyRotation();

            if(_impulse.Progress >= 1) break;

            yield return new WaitForFixedUpdate();
        }
        
    }
    private bool CalculateApprocach(Vector3 position, out Vector3 key)
    {
        if(_commands.Keys.IsEmpty())
        {
            key = Vector3.negativeInfinity;
            return false;
        }
        var keys = _commands.Keys.ToArray();
        var threshold = 0.25f;
        for(var i = 0; i < keys.Length; i++)
        {
            if ((position - keys[i]).sqrMagnitude < threshold * threshold)
            {
                key = keys[i];
                return true;
            }
        }
        key = Vector3.negativeInfinity;
        return false;
    }

    private Dictionary<Vector3, Action> Predict(Vector3 start, List<IContributable<Vector3>> contributable)
    {
        var result = new Dictionary<Vector3, Action>();
        _currentPosition = start;

        _gravity.SetTimeflow(1);
        _impulse.SetTimeflow(1);
        _impulse.Apply(_impulseForce, _atenuationTime);

        var deltaTime = 0.02f;

        while (true)
        {
            _isGrounded = CheckGround(_currentPosition);

            

            if (_isGrounded && !_isPreviousGrounded)
            {
                var velocity = _gravity.Velocity;
                result[_currentPosition] = () =>
                {
                    _gravity.Enable();
                    _gravity.SetVelocity(velocity);
                };
            }
            if (!_isGrounded && _isPreviousGrounded)
            {
                var velocity = _gravity.Velocity;
                result[_currentPosition] = () =>
                {
                    _gravity.Zero();
                    _gravity.Disable();
                };
            }
            _currentPosition += CalculateMovement(deltaTime);

            _isPreviousGrounded = _isGrounded;
            if (_impulse.Progress == 1) break;
        }
        return result;
    }
    private Vector3 CalculateMovement(float deltaTime)
    {
        TickContributables(deltaTime);

        if (_isGrounded)
        {
            _gravity.Disable();
        }
        else
        {
            if (_gravity.Enabled == false)
            {
                _gravity.Zero();
            }
            _gravity.Enable();
        }
        if (_impulse.Progress >= 1)
        {
            _impulse.Disable();
        }
        else
        {
            _impulse.Enable();
        }
        var gravity = _gravity.Contribute;
        var impulse = _impulse.Contribute;
        var result = gravity + impulse;

        if (_isGrounded)
        {
            //result -= impulse * 0.25f;
        }
        return result;
    }
    private void TickContributables(float deltaTime)
    {
        for (var i = 0; i < _contributables.Count; i++)
        {
            var contribute = _contributables[i];
            if (contribute.Enabled) contribute.Tick(deltaTime);
        }
    }
    private bool CheckGround(Vector3 point)
    {
        var hit = Physics.SphereCast(point, 0.75f * (_collider.bounds.size.x / 2), Vector3.down, out var hitInfo, 0.4f,LayerMask.GetMask("Default", "Ground"));

        return hit;
    }
}


