using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : IDisposable, IInput
{
    public event Action CameraReset;
    public event Action Space;
    public event Action<bool> Pulling;
    
    
    private InputActions _inputs;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Gameplay.Space.performed += SpaceHandle;
        _inputs.Gameplay.Pulling.started += PullHandle;
        _inputs.Gameplay.Pulling.canceled += PullHandle;
        _inputs.Gameplay.CameraReset.started += CameraResetHandle;
    }

    public void Dispose()
    {
        _inputs.Gameplay.Space.performed -= SpaceHandle;
        _inputs.Gameplay.Pulling.started -= PullHandle;
        _inputs.Gameplay.Pulling.canceled -= PullHandle;
        _inputs.Gameplay.CameraReset.started -= CameraResetHandle;

        _inputs.Disable();
        _inputs?.Dispose();
    }
    public Vector2 GetAiming() => _inputs.Gameplay.Aiming.ReadValue<Vector2>();
    public Vector2 GetDirection() => _inputs.Gameplay.Direction.ReadValue<Vector2>();
    private void SpaceHandle(InputAction.CallbackContext context) => Space?.Invoke();
    private void PullHandle(InputAction.CallbackContext context) => Pulling?.Invoke(context.started);
    private void CameraResetHandle(InputAction.CallbackContext context) => CameraReset?.Invoke();

}