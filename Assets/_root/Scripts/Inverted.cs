using Components;
using R3.Triggers;
using System.Collections.Generic;
using UnityEngine;

public class Inverted : MonoBehaviour
{
    private Transform _transform;
    private CharacterController _characterController;

    private Gravity _gravity;
    private GroundChecker _groundChecker;

    private Vector3 _velocity;
    
    private bool _isGrounded;


    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _characterController = GetComponent<CharacterController>();
        _gravity = new Gravity();
        _groundChecker = new GroundChecker(LayerMask.GetMask("Default", "Ground"));
    }
    private void Update()
    {
        if (Time.frameCount % 2 == 0) _isGrounded = CheckGround();

        if (_isGrounded) _gravity.Reset(Vector3.zero);
        _velocity = Vector3.zero;

        _velocity += _gravity.Apply();

        _characterController.Move(_velocity);
    }
    private bool CheckGround()
    {
        return _groundChecker.Check(transform.position + Vector3.down * 0.55f, 0.045f);
    }
    struct TransformData
    {
        public Vector3 Pos;
        public Vector3 Rot;
    }
}
