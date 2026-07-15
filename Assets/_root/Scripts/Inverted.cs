using Components;
using ModestTree;
using NUnit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inverted : MonoBehaviour
{
    [SerializeField] private Vector3 _impulseForce = new Vector3(-4f, 0, 0);
    [SerializeField] private float _atenuationTime = 1;

    private Transform _transform;
    private Rigidbody _rigidbody;

    private Gravity _gravity;
    private Impulse _impulse;
    private Collider _collider;
    
    private Stack<Command> _commands;

    private Timeflow _timeflow;

    private List<ITickable> _contributables;
    private Vector3 _currentPosition;
    private Vector3 _initPosition;
    private Vector3 _normal;

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
        var axis = -new Vector3(-_impulse.Contribute.z, _impulse.Contribute.y, _impulse.Contribute.x);
        var angle = axis.magnitude * 3200 * Time.deltaTime;
        _rigidbody.MoveRotation(Quaternion.AngleAxis(angle, axis.normalized) * _rigidbody.rotation);
    }

    private IEnumerator InvertRoutine(List<IContributable<Vector3>> contributables, Vector3 to)
    {
        _commands =  Predict(to, _contributables.Cast<IContributable<Vector3>>().ToList());

        _impulse.SetTimeflow(_timeflow.Relative);
        _gravity.SetTimeflow(_timeflow.Relative);

        _impulse.Disable();
        _impulse.Enable();

        _gravity.SetDirection(-_gravity.Direction);
        _gravity.Disable();

        _currentPosition = _rigidbody.position;

        _commands.Pop().Action.Invoke();


        while (true)
        {
            TickContributables(Time.fixedDeltaTime);

            _currentPosition += _impulse.Contribute + _gravity.Contribute;

            _rigidbody.MovePosition(_currentPosition);

            if (CalculateApprocach(_currentPosition, _commands.Peek().Position))
            {
                _rigidbody.position = _currentPosition;
                _commands.Pop().Action.Invoke();
            }

            ApplyRotation();

            if (_commands.IsEmpty()) break;

            yield return new WaitForFixedUpdate();
        }

        _rigidbody.position = to;
        _currentPosition = _rigidbody.position;
        _gravity.Disable();
        _impulse.Disable();

    }
    private bool CalculateApprocach(Vector3 position, Vector3 next)
    {
        var threshold = 0.8f;
        return (position - next).sqrMagnitude < threshold * threshold;
    }

    private Stack<Command> Predict(Vector3 to, List<IContributable<Vector3>> contributable)
    {
        _collider.enabled = false;
        var result = new Stack<Command>();
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

            if (_impulse.Force == _impulseForce && _impulse.Progress == 0)
            {
                result.Push(new Command("impulse off",
                    () =>
                {
                    _impulse.Disable();
                }, _currentPosition, _gravity.Contribute, _impulse.Contribute));
            }

            if (_isGrounded && !_isPreviousGrounded)
            {
                var velocity = _gravity.Velocity;

                result.Push(new Command("gravity on",
                    () =>
                    {
                        _gravity.Enable();
                        _gravity.SetVelocity(velocity);
                    }, _currentPosition, _gravity.Contribute, _impulse.Contribute));

            }
            if (!_isGrounded && _isPreviousGrounded)
            {
                var velocity = _gravity.Velocity;

                result.Push(new Command("gravity off",
                    () =>
                    {
                        _gravity.Disable();
                    }, _currentPosition, _gravity.Contribute, _impulse.Contribute));

            }
            if (_isForwardHit && !_isPreviousForwardHit)
            {
                var end = -_impulse.Contribute;
                var start = -_impulse.Force;
                var atenuationTime = _impulse.AtenuationTime * _impulse.Progress;

                result.Push(new Command("impusle change",
                    () =>
                    {
                        _impulse.Apply(start, end, atenuationTime);
                    }, _currentPosition, _gravity.Contribute, _impulse.Contribute));

            }
            if (_impulse.Progress == 1)
            {
                var force = -_impulse.Force;
                var atenuationTime = _impulse.AtenuationTime;

                result.Push(new Command("impusle last",
                    () =>
                    {
                        _impulse.Apply(force, atenuationTime);
                    }, _currentPosition, _gravity.Contribute, _impulse.Contribute));

                break;
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
            _impulse.Apply(Vector3.Reflect(_impulse.Contribute, _normal), _impulse.AtenuationTime * _impulse.Progress);
        }
        
        var gravity = _gravity.Contribute;
        var impulse = _impulse.Contribute;
        var result = gravity + impulse;

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
        var hit = Physics.Raycast(point, direction, out var hitInfo, 0.65f, LayerMask.GetMask("Default", "Ground"));

        if (hit)
        {
            
            _normal = hitInfo.normal;
            return hit;
        }
        _normal = Vector3.zero;
        return false;
    }
    public class Command
    {
        public string Name;
        public Action Action;
        public Vector3 Position;
        public Vector3 Gravity;
        public Vector3 Impulse;

        public Command(string name, Action action, Vector3 position, Vector3 gravity, Vector3 impulse)
        {
            Name = name;
            Action = action;
            Position = position;
            Gravity = gravity;
            Impulse = impulse;
        }
    }
}

