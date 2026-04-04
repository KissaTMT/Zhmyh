using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : IDisposable, IInput
{
    public event Action CameraReset;
    public event Action Dash;
    public event Action Jump;
    public event Action<bool> Pulling;
    
    
    private InputActions _inputs;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Gameplay.Dash.performed += DashHandle;
        _inputs.Gameplay.Jump.started += JumpHandle;
        _inputs.Gameplay.Pulling.started += PullHandle;
        _inputs.Gameplay.Pulling.canceled += PullHandle;
        _inputs.Gameplay.CameraReset.started += CameraResetHandle;
    }
    public void Dispose()
    {
        _inputs.Gameplay.Dash.performed -= DashHandle;
        _inputs.Gameplay.Jump.started -= JumpHandle;
        _inputs.Gameplay.Pulling.started -= PullHandle;
        _inputs.Gameplay.Pulling.canceled -= PullHandle;
        _inputs.Gameplay.CameraReset.started -= CameraResetHandle;

        _inputs.Disable();
        _inputs?.Dispose();
    }
    public Vector2 GetLook() => _inputs.Gameplay.Look.ReadValue<Vector2>();
    public Vector2 GetMove() => _inputs.Gameplay.Move.ReadValue<Vector2>();
    private void DashHandle(InputAction.CallbackContext context) => Dash?.Invoke();
    private void JumpHandle(InputAction.CallbackContext context) => Jump?.Invoke();
    private void PullHandle(InputAction.CallbackContext context) => Pulling?.Invoke(context.started);
    private void CameraResetHandle(InputAction.CallbackContext context) => CameraReset?.Invoke();

}