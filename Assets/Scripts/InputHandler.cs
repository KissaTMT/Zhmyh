using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : IDisposable, IInput
{
    public event Action<Vector2> OnMovement;
    public event Action<Vector2> OnAim;
    public event Action OnJump;

    private InputActions _inputs;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Gameplay.Movement.performed += movement => OnMovement?.Invoke(movement.ReadValue<Vector2>());
        _inputs.Gameplay.Movement.canceled += movement => OnMovement?.Invoke(Vector2.zero);

        _inputs.Gameplay.Movement.performed += aim => OnAim?.Invoke(aim.ReadValue<Vector2>());

        _inputs.Gameplay.Jump.performed += jump => OnJump?.Invoke();
    }
    public void Dispose()
    {
        _inputs.Disable();

        _inputs.Gameplay.Movement.performed -= movement => OnMovement?.Invoke(movement.ReadValue<Vector2>());
        _inputs.Gameplay.Movement.canceled -= movement => OnMovement?.Invoke(Vector2.zero);

        _inputs.Gameplay.Movement.performed -= aim => OnAim?.Invoke(aim.ReadValue<Vector2>());

        _inputs.Gameplay.Jump.performed -= jump => OnJump?.Invoke();
    }
}
