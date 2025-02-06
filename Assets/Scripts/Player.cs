using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Zhmyh _zhmyh;
    private IInput _input;
    private Vector3 _Aimposition;

    private void Awake()
    {
        _input = new InputHandler();

        _zhmyh = GetComponent<Zhmyh>().Init();

        _input.OnMovement += SetDirection;
        _input.OnJump += Jump;
    }

    private void OnDisable()
    {
        _input.OnMovement -= SetDirection;
        _input.OnJump -= Jump;
    }
    private void SetDirection(Vector2 vector2) => _zhmyh.SetDirection(vector2);
    private void Jump() => _zhmyh.Jump();
    private void Update()
    {
        _Aimposition = Mouse.current.position.ReadValue();
        _Aimposition.z = Camera.main.nearClipPlane;
    }
    private void FixedUpdate()
    {
        _zhmyh.Move();
        _zhmyh.RotateHands(_Aimposition);
    }
}
