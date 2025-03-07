using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float _speed;
    private Transform _transform;
    private Player _player;

    [Inject]
    public void Construct(Player player)
    {
        _player = player;
        
    }
    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    private void Update()
    {
        _transform.position = Vector3.Lerp(_transform.position, new Vector3(_player.Transform.position.x, _player.Transform.position.y,_transform.position.z) + (Vector3)_offset, _speed * Time.deltaTime);
    }
}
