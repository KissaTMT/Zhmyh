using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float _speed;
    private Transform _transform;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    private void Update()
    {
        _transform.position = Vector3.Lerp(_transform.position, new Vector3(_target.position.x,_target.position.y,_transform.position.z) + (Vector3)_offset, _speed * Time.deltaTime);
    }
}
