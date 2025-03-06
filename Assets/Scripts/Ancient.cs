using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ancient : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed;

    private Transform _transform;
    private Rigidbody2D _rigidbody;


    private NPhMovementController _movementController;

    private Vector2 _currentDirection;
    public Ancient Init()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _movementController = new NPhMovementController(_transform, _speed);
        return this;
    }
    public void Move()
    {
        _animator.SetBool("IsRun", _currentDirection.magnitude > 0.1f);
        if (_currentDirection.x != 0 && Mathf.Sign(_currentDirection.x) != Mathf.Sign(_transform.localScale.x)) _transform.localScale = new Vector3(-1 * _transform.localScale.x, _transform.localScale.y, _transform.localScale.z);
        _movementController.Move(_currentDirection);
    }
    public void SetDirection(Vector2 direction) => _currentDirection = direction;
    
    private void Awake()
    {
        Init();
    }
    private void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.nearClipPlane;
        Vector3 Worldpos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = Worldpos;
        //_currentDirection = new Vector2(_target.position.x - _transform.position.x, _target.position.z - _transform.position.z).normalized;
    }
    private void FixedUpdate()
    {
        //Move();
    }
}
