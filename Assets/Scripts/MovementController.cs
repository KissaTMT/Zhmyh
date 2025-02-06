using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController
{
    private Transform _transform;
    private Rigidbody _rb;
    private float _speed;
    public MovementController(Transform transform, Rigidbody rb, float speed)
    {
        _transform = transform;
        _rb = rb;
        _speed = speed;
    }
    public Vector3 Move(Vector2 direction)
    {
        var delta = new Vector3(direction.x,0,direction.y) * _speed * Time.deltaTime;
        _rb.MovePosition(_rb.position + delta);
        return delta;
    }
}
