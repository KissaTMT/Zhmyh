using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float Speed => _speed;
    public float GravityScale => _gravityScale;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float _speed = 2048;
    [SerializeField] private float _gravityScale = 256;
    [SerializeField] private float _radiusOverlapSphere = 0.5f;
    [SerializeField] private float _radiusSphereCast = 0.1f;

    private Transform _transform;
    private Collider[] _buffer = new Collider[1];

    private Vector3 _currentVelocity;
    private Vector3 _previousPosition;

    private float _flightTime;
    public void Init(Vector3 direction, float impulseForce = 1)
    {
        _transform = GetComponent<Transform>();
        _previousPosition = _transform.position;

        _currentVelocity = direction * _speed * impulseForce;
    }

    private void FixedUpdate()
    {
        _previousPosition = transform.position;
        Flight();
        CheckCollisions();
    }

    private void Flight()
    {
        _flightTime += Time.deltaTime;
        _currentVelocity += Vector3.down * 0.1f * _gravityScale * _flightTime * _flightTime;
        _transform.position += _currentVelocity * Time.deltaTime;
    }
    private void CheckCollisions()
    {
        var delta = _transform.position - _previousPosition;
        var distance = delta.magnitude;

        if (distance > float.Epsilon * 100 &&
            Physics.SphereCast(_transform.position, _radiusSphereCast, delta.normalized, out var hitInfo, distance, _layerMask))
        {
            HitHandle(hitInfo.collider);
            return;
        }

        if (Physics.OverlapSphereNonAlloc(_transform.position, _radiusOverlapSphere, _buffer, _layerMask) > 0) HitHandle(_buffer[0]);
    }
    private void HitHandle(Collider hit)
    {
        var unit = hit.transform.GetComponentInParent<Unit>();

        if (unit != null)
        {
            var point = Physics.ClosestPoint(_transform.position, hit, hit.transform.position, Quaternion.identity)
                - _currentVelocity.normalized * hit.bounds.size.x;
            unit.Health.Decrement();
            var blood = Instantiate(_particleSystem, point, Quaternion.identity);
            blood.startSize *= 2;
        }
        Destroy(gameObject);
    }
}