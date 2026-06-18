using Components;
using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    
    private Dictionary<Vector3, (string name, Action command)> _commands;

    private Timeflow _timeflow;

    private List<ITickable> _contributables;
    private Vector3 _currentPosition;
    private Vector3 _initPosition;

    private bool _isGrounded;
    private bool _isPreviousGrounded;

    private bool _isForwardHit;
    private bool _isPreviousForwardHit;
    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _gravity = new Gravity();
        _impulse = new Impulse();
        _gravity.Zero();
        _initPosition = _rigidbody.position;
        _contributables = new List<ITickable>
        {
            _gravity,
            _impulse
        };

        _timeflow = new Timeflow().SetRelativeFlow(new Timeflow());

        
    }
    private void Start()
    {
        StartCoroutine(ImpulseRoutine());
        _currentPosition = _transform.position;
        _isPreviousGrounded = true;
        _isPreviousForwardHit = false;
    }

    private IEnumerator ImpulseRoutine()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            _gravity.SetTimeflow(_timeflow.Relative);
            _gravity.Enable();
            _gravity.Zero();
            _gravity.SetDirection(Vector3.down);

            
            _impulse.SetTimeflow(_timeflow.Relative);
            _impulse.Apply(_impulseForce, _atenuationTime);
            _impulse.Enable();

            yield return new WaitForSeconds(4f);
            _timeflow.Inverse();
            yield return StartCoroutine(InvertRoutine(_contributables.Cast<IContributable<Vector3>>().ToList(), _initPosition));
            yield return new WaitForSeconds(1);
            _timeflow.Inverse();
        }
    }
    private void FixedUpdate()
    {
        if (_timeflow.Relative == -1) return;

        _isGrounded = CheckGround(_currentPosition);
        _isForwardHit = CheckForwardObstacle(_currentPosition, _impulse.Force.normalized);

        var delta = CalculateMovement(Time.fixedDeltaTime);

        _currentPosition += delta;

        _rigidbody.MovePosition(_currentPosition);


        ApplyRotation();

        _isPreviousGrounded = _isGrounded;
        _isPreviousForwardHit = _isForwardHit;
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

        _impulse.SetTimeflow(_timeflow.Relative);
        _gravity.SetTimeflow(_timeflow.Relative);

        _impulse.Disable();
        _impulse.Enable();

        _gravity.SetDirection(-_gravity.Direction);
        _gravity.Disable();

        _currentPosition = _rigidbody.position;
        var keys = _commands.Keys.ToList();
        _commands[keys[_commands.Count - 1]].command.Invoke();

        var currentIndex = keys.Count - 2;


        while (true)
        {
            TickContributables(Time.fixedDeltaTime);

            _currentPosition += _impulse.Contribute + _gravity.Contribute;

            _rigidbody.MovePosition(_currentPosition);

            if (CalculateApprocach(_transform.position, keys[currentIndex]))
            {
                _rigidbody.position = _currentPosition;
                _commands[keys[currentIndex]].command.Invoke();
                currentIndex--;
            }

            ApplyRotation();

            if (currentIndex < 0) break;

            yield return new WaitForFixedUpdate();
        }

        _rigidbody.position = to;
        _currentPosition = _rigidbody.position;
        _gravity.Disable();
        _impulse.Disable();

    }
    private bool CalculateApprocach(Vector3 position, Vector3 next)
    {
        var threshold = 0.4f;
        if ((position - next).sqrMagnitude < threshold * threshold)
        {
            return true;
        }
        return false;
    }

    private Dictionary<Vector3, (string name, Action command)> Predict(Vector3 to, List<IContributable<Vector3>> contributable)
    {
        _collider.enabled = false;
        var result = new Dictionary<Vector3, (string name, Action command)>();
        _currentPosition = to;

        _gravity.SetTimeflow(1);
        _impulse.SetTimeflow(1);
        _impulse.Apply(_impulseForce, _atenuationTime);

        var deltaTime = 0.02f;

        _isPreviousGrounded = true;
        _isPreviousForwardHit = false;


        while (true)
        {
            _isGrounded = CheckGround(_currentPosition);
            _isForwardHit = CheckForwardObstacle(_currentPosition, _impulse.Force.normalized);

            if (_impulse.Force == _impulseForce && _impulse.Progress < float.Epsilon)
            {
                if (result.ContainsKey(_currentPosition))
                {
                    var t = result[_currentPosition];
                    t.name += "impusle off";
                    t.command += () =>
                    {
                        _impulse.Disable();
                    };
                    result[_currentPosition] = t;
                }
                else
                {
                    result[_currentPosition] = ("impusle off", () =>
                    {
                        _impulse.Disable();
                    }
                    );
                }
            }

            if (_isGrounded && !_isPreviousGrounded)
            {
                var velocity = _gravity.Velocity;

                if (result.ContainsKey(_currentPosition))
                {
                    var t = result[_currentPosition];
                    t.name += "gravity on";
                    t.command += () =>
                    {
                        _gravity.Enable();
                        _gravity.SetVelocity(velocity);
                    };
                    result[_currentPosition] = t;
                }
                else
                {
                    result[_currentPosition] = ("gravity on", () =>
                    {
                        _gravity.Enable();
                        _gravity.SetVelocity(velocity);
                    }
                    );
                }
            }
            if (!_isGrounded && _isPreviousGrounded)
            {
                var velocity = _gravity.Velocity;

                if (result.ContainsKey(_currentPosition))
                {
                    var t = result[_currentPosition];
                    t.name += "gravity off";
                    t.command += () =>
                    {
                        _gravity.Disable();
                    };
                    result[_currentPosition] = t;
                }
                else
                {
                    result[_currentPosition] = ("gravity off", () =>
                    {
                        _gravity.Disable();
                    }
                    );
                }
            }
            if (_isForwardHit && !_isPreviousForwardHit)
            {
                var end = -_impulse.Contribute;
                var start = -_impulse.Force;
                var atenuationTime = _impulse.AtenuationTime * _impulse.Progress * 1.2f;

                if (result.ContainsKey(_currentPosition))
                {
                    var t = result[_currentPosition];
                    t.name += "impusle change";
                    t.command += () =>
                    {
                        _impulse.Apply(start, end, atenuationTime);
                    };
                    result[_currentPosition] = t;
                }
                else
                {
                    result[_currentPosition] = ("impusle change", () =>
                    {
                        _impulse.Apply(start, end, atenuationTime);
                    }
                    );
                }
            }
            if (_impulse.Progress == 1)
            {
                var force = -_impulse.Force;
                var atenuationTime = _impulse.AtenuationTime;

                if (result.ContainsKey(_currentPosition))
                {
                    var t = result[_currentPosition];
                    t.name += "impusle last";
                    t.command += () =>
                    {
                        _impulse.Apply(force, atenuationTime);
                    };
                    result[_currentPosition] = t;
                }
                else
                {

                    result[_currentPosition] = ("impusle last", () =>
                    {
                        _impulse.Apply(force, atenuationTime);
                    }
                    );
                    break;
                }
            }
            
            _currentPosition += CalculateMovement(deltaTime);

            _isPreviousGrounded = _isGrounded;
            _isPreviousForwardHit = _isForwardHit;

        }
        _collider.enabled = true;
        _impulse.Disable();
        _gravity.Disable();
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

        if (_isForwardHit)
        {
            _impulse.Apply(-_impulse.Contribute, _impulse.AtenuationTime * _impulse.Progress * 0.8f);
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
        var hit = Physics.SphereCast(point, 0.75f * (_collider.bounds.size.x / 2), Vector3.down, out var hitInfo, 0.4f, LayerMask.GetMask("Default", "Ground"));

        return hit;
    }
    private bool CheckForwardObstacle(Vector3 point, Vector3 direction)
    {
        var hit = Physics.Raycast(point, direction, out var hitInfo,1, LayerMask.GetMask("Default", "Ground"));

        return hit;
    }
}

