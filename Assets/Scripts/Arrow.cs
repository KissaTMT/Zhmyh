using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Transform _transform;
    private Vector2 _target;

    public void Init(Vector2 target)
    {
        _transform = GetComponent<Transform>();
        _target = target;
    }
    private void Update()
    {
        _transform.position = Vector2.MoveTowards(_transform.position, _target, _speed * Time.deltaTime);
        if(Vector2.Distance(_transform.position,_target)<0.01f) Destroy(gameObject);
    }
}