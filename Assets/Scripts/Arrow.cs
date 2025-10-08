using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;

public class Arrow : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float _speed = 1024;
    [SerializeField] private float _gravityScale = 256;

    private Transform _transform;
    private Collider[] _buffer = new Collider[4];
    
    private Vector3 _velocity;
    
    public void Init(Vector3 direction, float impulseForce = 1)
    {
        _transform = GetComponent<Transform>();
        _velocity = direction * _speed * impulseForce;

        StartCoroutine(DestroyRoutine());
    }
    private void Update()
    {
        Flihgt();
        CheckCollisions();
    }
    private void Flihgt()
    {
        _velocity += _gravityScale * Vector3.down * Time.deltaTime;
        _transform.position += _velocity * Time.deltaTime;
    }
    private void CheckCollisions()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(_transform.position, 0.6f, _buffer, _layerMask);

        for (int i = 0; i < hitCount; i++)
        {
            var unit = _buffer[i].transform.GetComponentInParent<Zhmyh>();
            if (unit != null)
            {
                Profiler.BeginSample("hit");
                var point = Physics.ClosestPoint(_transform.position, _buffer[i], _buffer[i].transform.position, Quaternion.identity) - _velocity.normalized * _buffer[i].bounds.size.x * 0.5f;
                unit.Health.Decrement();
                Profiler.EndSample();
                Profiler.BeginSample("inst pert");
                var blood = Instantiate(_particleSystem, point, Quaternion.identity);
                Profiler.EndSample();
                Profiler.BeginSample("desrt");
                Destroy(gameObject);
                Profiler.EndSample();
                break;
            }
        }
    }
    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}