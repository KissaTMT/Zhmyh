using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gniling : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private ShiftConfig[] _configs;
    [SerializeField] private GameObject _aim;
    private Transform _transform;

    private IMovementController _movement;
    private Shifter _shifter;

    private Vector2 _currentDirection;
    private Vector2 _facingDirection;

    public Gniling Init()
    {
        _transform = GetComponent<Transform>();
        _movement = new NPhMovementController(_transform, _speed);
        _shifter = new Shifter(_transform, _configs, GetComponentInChildren<SpriteSorterRenderer>());
        return this;
    }
    public void SetDirection(Vector2 input) => _currentDirection = input;
    public void Move()
    {
        _movement.Move(_currentDirection);
        _shifter.Shift(_facingDirection);
    }
    public void RotateHands(Vector2 vector)
    {
        var screenPosition = Camera.main.WorldToScreenPoint(_transform.position);
        var delta = vector - (Vector2)screenPosition;
        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        _aim.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        _facingDirection = delta;
    }
}
