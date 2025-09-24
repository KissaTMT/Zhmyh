using System.Collections;
using System.Linq;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private float _speed;
    private Transform _transform;
    private Vector3 _target;

    public void Init(Vector3 target)
    {
        _transform = GetComponent<Transform>();
        _target = target;
        StartCoroutine(DestroyRoutine());
    }
    private void Update()
    {
        _transform.position = Vector3.MoveTowards(_transform.position, _target, _speed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        var unit = other.GetComponentInParent<Zhmyh>();
        if (unit)
        {
            unit.Health.Decrement();
            Instantiate(_particleSystem, _transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}