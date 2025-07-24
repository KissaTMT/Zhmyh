using UnityEngine;

public class NPhMovementController : IMovementController
{
    private Transform _transform;
    private float _speed;
    public NPhMovementController(Transform transform, float speed)
    {
        _transform = transform;
        _speed = speed;
    }
    public Vector2 Move(Vector2 direction)
    {
        var delta = direction * _speed * Time.deltaTime;
        _transform.position += new Vector3(delta.x, 0, delta.y);
        return delta;
    }
}
