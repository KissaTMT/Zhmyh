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
        _transform.position = Vector3.MoveTowards(GetDirection(_transform.position), GetDirection(_target), _speed * Time.deltaTime);
        if(Vector2.Distance(GetDirection(_transform.position), GetDirection(_target)) <0.01f) Destroy(gameObject);
    }
    private Vector3 GetDirection(Vector2 vector) => new Vector3(vector.x, _transform.position.y, vector.y);
}