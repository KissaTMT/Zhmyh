using NaughtyAttributes.Test;
using System.Collections;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public SwordState State => _state;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float _radiusOverlapSphere = 8f;
    [SerializeField] private float _flightSpeed = 1024;
    [SerializeField] private float _rotationSpeed = 64;

    private Unit _owner;
    private Transform _handle;
    private Transform _transform;
    
    private Vector3 _ownStatePosition;
    private Vector3 _ownStateEulers;
    private Vector3 _currentDirection;

    private SwordState _state = SwordState.Own;

    private Collider[] _buffer = new Collider[1];

    private Coroutine _flight;
    public void SetDirection(Vector3 direction)
    {
        _currentDirection = direction;
    }
    public void Cast()
    {
        if (_flight != null) return;
        _flight = StartCoroutine(FlightRoutine());
    }
    public void Return()
    {
        if (_flight != null) return;
        _flight = StartCoroutine(ReturnRoutine());
    }
    public void Init(Unit owner, Transform handle)
    {
        _transform = GetComponent<Transform>();
        _owner = owner;
        _handle = handle;
        _ownStatePosition = transform.localPosition;
        _ownStateEulers = transform.localEulerAngles;
    }
    private void Flight(Vector3 direction)
    {
        _transform.position += direction * _flightSpeed * Time.deltaTime;
        _transform.eulerAngles = 
            new Vector3(_transform.eulerAngles.x, _transform.eulerAngles.y + _rotationSpeed * Time.deltaTime, _transform.eulerAngles.z);
    }
    private void CheckCollision()
    {
        if (Physics.OverlapSphereNonAlloc(_transform.position, _radiusOverlapSphere, _buffer, _layerMask) > 0) HitHandle(_buffer[0]);
    }
    private IEnumerator FlightRoutine()
    {
        _state = SwordState.Cast;
        _transform.parent = null;
        _transform.eulerAngles = new Vector3(90, 0, -65);
        var destination = _transform.position + _currentDirection * 1024;
        var direction = _currentDirection;
        while((destination - _transform.position).magnitude > 64)
        {
            Flight(direction);
            CheckCollision();
            yield return null;
        }
        _flight = null;
        Return();
    }
    private IEnumerator ReturnRoutine()
    {
        if (_handle == null) yield break;
        while ((_handle.position - _transform.position).magnitude > 32)
        {
            if (_handle == null) yield break;
            Flight((_handle.position - transform.position).normalized);
            CheckCollision();
            yield return null;
        }
        _transform.parent = _handle;
        _transform.localEulerAngles = _ownStateEulers;
        _transform.localPosition = _ownStatePosition;
        _state = SwordState.Own;
        _flight = null;
    }
    private void HitHandle(Collider hit)
    {
        var unit = hit.GetComponentInParent<Unit>();
        if (unit != null)
        {
            if (unit == _owner) return;

            unit.Health.Decrement();
            var blood = Instantiate(_particleSystem, unit.Transform.position, Quaternion.identity);
            blood.startSize *= 2;
        }
        Return();
    }
    public enum SwordState
    {
        Own,
        Cast
    }
}
