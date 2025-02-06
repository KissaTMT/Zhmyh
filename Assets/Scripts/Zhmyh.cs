using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zhmyh : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _groundChecker;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private GameObject[] _body;
    [SerializeField] private GameObject _aim;
    
    private Transform _transform;
    private Rigidbody _rigidbody;
    
    private MovementController _movementController;
    private Shifter _shifter;

    private Vector2 _currentDirection;
    private Vector2 _facingDirection;
    public Zhmyh Init()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _movementController = new MovementController(_transform, _rigidbody, _speed);
        _shifter = new Shifter(_transform);
        return this;
    }
    public void SetDirection(Vector2 input)=> _currentDirection = input;
    public void Jump()
    {
        if (Physics.Raycast(_groundChecker.position, -_transform.up, 1))
        {
            _rigidbody.AddForce(_transform.up * _jumpForce * 100);
        }
    }
    public void Move()
    {
        _animator.SetBool("IsRun", _currentDirection.magnitude > 0.1f);
        if (Mathf.Sign(_facingDirection.x) != Mathf.Sign(_body[0].transform.localScale.x)) Flip();
        _movementController.Move(_currentDirection);
    }
    private void Flip()
    {
        for(var i = 0; i < _body.Length; i++)
        {
            var bodyTransform = _body[i].transform;
            bodyTransform.localScale = new Vector3(-1 * bodyTransform.localScale.x, bodyTransform.localScale.y, bodyTransform.localScale.z);
        }
    }
    public void RotateHands(Vector2 vector)
    {
        var screenPosition = Camera.main.WorldToScreenPoint(_transform.position);
        var delta = vector - (Vector2)screenPosition;
        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        _aim.transform.rotation = Quaternion.Euler(45, 0f, angle);
        _facingDirection = delta;

    }
}
