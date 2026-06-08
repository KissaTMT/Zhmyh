using Components;
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

        _buffer = Predict(new Vector3(-7, 6.5f, 23), _impulseForce);

        _gravity.Zero();
        _gravity.Enable();

        StartCoroutine(ImpulseRoutine());
    }

    private IEnumerator ImpulseRoutine()
    {
        yield return new WaitForSeconds(1);
        _impulse.Init(_impulseForce, _atenuationTime);
        yield return new WaitUntil(() => _impulse.Value == Vector3.zero);
        yield return new WaitForSeconds(0.25f);
        _coroutine = StartCoroutine(InvertRoutine());
    }
    private void FixedUpdate()
    {
        if (_coroutine != null) return;

        _isGrounded = CheckGround();
        _impulse.Tick(Time.deltaTime);

        if (_isGrounded)
        {
            _gravity.Zero();
        }
        var gravity = _gravity.Apply(Time.deltaTime) / Time.deltaTime;
        var impulse = _impulse.Value;
        var result = gravity + impulse;
        _rigidbody.linearVelocity = result;
    }

    private IEnumerator InvertRoutine()
    {
        if (_buffer.Count < 2) yield break;

        // Начинаем с последней точки буфера
        _rigidbody.position = _buffer[_buffer.Count - 1];

        _rigidbody.linearVelocity = Vector3.zero;

        // Проходим от последней точки к первой
        for (int i = _buffer.Count - 1; i > 0; i--)
        {
            Vector3 target = _buffer[i - 1];          // следующая точка на пути назад
            Vector3 delta = target - _rigidbody.position ;

            // Вычисляем скорость: delta / желаемое время движения
            // Если буфер заполнялся с фиксированным интервалом (например, каждый FixedUpdate),
            // используем Time.fixedDeltaTime как время прохождения сегмента.
            float segmentTime = 0.1f;   // или задайте свою константу
            Vector3 velocity = delta / segmentTime;

            _rigidbody.linearVelocity = velocity + Vector3.down * 0.25f;

            // Ждём, пока объект не достигнет целевой точки (с небольшим допуском)
            while (Vector3.Distance(_rigidbody.position, target) > 0.05f)
            {
                yield return new WaitForFixedUpdate();
            }
            _rigidbody.position = target;
        }

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    private List<Vector3> Predict(Vector3 start, Vector3 impulse)
    {
        var result = new List<Vector3>();

        var position = start;

        _impulse.Init(_impulseForce, _atenuationTime);

        var delta = 0.1f;

        result.Add(start);

        while (true)
        {
            _isGrounded = _groundChecker.Check(position + Vector3.down * 0.5f, 0.1f);
            _impulse.Tick(delta);

            if (_isGrounded)
            {
                _gravity.Zero();
                _gravity.Disable();
            }
            else _gravity.Enable();

            var gravity = _gravity.Apply(delta/3) / delta;
            position += gravity + _impulse.Value * delta;

            if (position.y < 0.5f) position.y = 0.5f;

            if (_impulse.Value == Vector3.zero) break;

            result.Add(position);
        }
        
        return result;
    }

    private bool CheckGround()
    {
        return _groundChecker.Check(transform.position + Vector3.down * 0.5f, 0.05f);
    }
}


