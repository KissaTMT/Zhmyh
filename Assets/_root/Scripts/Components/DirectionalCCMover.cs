using System;
using UnityEngine;

public class DirectionalCCMover : IMover
{
    private CharacterController _characterController;
    private float _speed;
    public DirectionalCCMover(CharacterController characterController, float speed)
    {
        _characterController = characterController;
        _speed = speed;
    }
    public Vector3 Move(Vector3 directtion)
    {
        var delta = directtion * _speed * Time.deltaTime;
        _characterController.Move(delta);
        return delta;
    }
}