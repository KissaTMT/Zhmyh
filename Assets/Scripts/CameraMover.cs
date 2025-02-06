using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _speed;
    private Transform _transform;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    private void FixedUpdate()
    {
        _transform.position = Vector3.Lerp(_transform.position, _target.position + _offset, _speed * Time.deltaTime);
    }
}
