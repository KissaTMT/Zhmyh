using UnityEngine;

public class DirectionalNotPhysicalMover : IMover
{
    private Transform _transform;
    private float _speed;
    public DirectionalNotPhysicalMover(Transform transform, float speed)
    {
        _transform = transform;
        _speed = speed;
    }
    public Vector3 Move(Vector3 direction)
    {
        var delta = direction * _speed * Time.deltaTime;
        _transform.position += delta;
        return delta;
    }
}
