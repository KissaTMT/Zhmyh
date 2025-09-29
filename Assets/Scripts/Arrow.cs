using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float _speed;
    private Collider[] _buffer;
    private Transform _transform;
    private Vector3 _destination;
    private Vector3 _source;
    private Vector3 _delta;
    

    public void Init(Vector3 destination)
    {
        _transform = GetComponent<Transform>();
        _source = _transform.position;
        _source.y = destination.y;
        _destination = destination;
        _delta = (_destination - _source).normalized;
        _buffer = new Collider[4];
        StartCoroutine(DestroyRoutine());
    }
    private void Update()
    {
        _transform.position += _delta * _speed * Time.deltaTime;
        if(Time.frameCount % 8 == 0) CheckCollisions();
    }
    private void CheckCollisions()
    {
        var hitCount = Physics.OverlapSphereNonAlloc(_transform.position, 0.5f, _buffer, _layerMask);

        for (int i = 0; i < hitCount; i++)
        {
            var unit = _buffer[i].transform.GetComponentInParent<Zhmyh>();
            if (unit != null)
            {
                var point = _buffer[i].bounds.center - _delta * _buffer[i].bounds.size.x * 0.75f;
                unit.Health.Decrement();
                Instantiate(_particleSystem, point, Quaternion.identity);
                Destroy(gameObject);
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