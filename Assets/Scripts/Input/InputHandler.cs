using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : IDisposable, IInput
{
    public event Action Space;
    public event Action<bool> Pulling;
    public event Action<bool> InitAiming;
    
    private InputActions _inputs;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Gameplay.Space.performed += SpaceHandle;
        _inputs.Gameplay.Pulling.started += PullHandle;
        _inputs.Gameplay.Pulling.canceled += PullHandle;
        _inputs.Gameplay.InitAiming.started += SetAimHandle;
        _inputs.Gameplay.InitAiming.canceled += SetAimHandle;
    }

    public void Dispose()
    {
        _inputs.Gameplay.Space.performed -= SpaceHandle;
        _inputs.Gameplay.Pulling.started -= PullHandle;
        _inputs.Gameplay.Pulling.canceled -= PullHandle;
        _inputs.Gameplay.InitAiming.started -= SetAimHandle;
        _inputs.Gameplay.InitAiming.canceled -= SetAimHandle;

        _inputs.Disable();
        _inputs?.Dispose();
    }
    public Vector2 GetAiming() => _inputs.Gameplay.Aiming.ReadValue<Vector2>();
    public Vector2 GetDirection() => _inputs.Gameplay.Direction.ReadValue<Vector2>();
    private void SpaceHandle(InputAction.CallbackContext context) => Space?.Invoke();
    private void PullHandle(InputAction.CallbackContext context) => Pulling?.Invoke(context.started ? true : false);
    private void SetAimHandle(InputAction.CallbackContext context) => InitAiming?.Invoke(context.started ? true : false);

}