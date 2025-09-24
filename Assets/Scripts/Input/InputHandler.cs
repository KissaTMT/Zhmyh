using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : IDisposable, IInput
{
    public event Action<Vector2> Direction;
    public event Action<Vector2> Aiming;
    public event Action Space;
    public event Action<bool> SetPull;
    public event Action<bool> SetAim;
    

    private InputActions _inputs;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Gameplay.Direction.performed += DirectionHandle;
        _inputs.Gameplay.Direction.canceled += ResetDirectionHandle;
        _inputs.Gameplay.Space.performed += SpaceHandle;
        _inputs.Gameplay.Pull.started += PullStartedHandle;
        _inputs.Gameplay.Pull.canceled += PullCanceledHandle;
        _inputs.Gameplay.SetAim.started += SetAimStartedHandle;
        _inputs.Gameplay.SetAim.canceled += SetAimCanceledHandle;
        _inputs.Gameplay.Aiming.performed += AimingHandle;
        _inputs.Gameplay.Aiming.canceled += ResetAimingHandle;
    }

    

    public void Dispose()
    {
        _inputs.Gameplay.Direction.performed -= DirectionHandle;
        _inputs.Gameplay.Direction.canceled -= ResetDirectionHandle;
        _inputs.Gameplay.Space.performed -= SpaceHandle;
        _inputs.Gameplay.Pull.started -= PullStartedHandle;
        _inputs.Gameplay.Pull.canceled -= PullCanceledHandle;
        _inputs.Gameplay.SetAim.started -= SetAimStartedHandle;
        _inputs.Gameplay.SetAim.canceled -= SetAimCanceledHandle;
        _inputs.Gameplay.Aiming.performed -= AimingHandle;
        _inputs.Gameplay.Aiming.canceled -= ResetAimingHandle;

        _inputs.Disable();
        _inputs?.Dispose();
    }
    private void DirectionHandle(InputAction.CallbackContext context) => Direction?.Invoke(context.ReadValue<Vector2>());
    private void ResetDirectionHandle(InputAction.CallbackContext context) => Direction?.Invoke(Vector2.zero);
    private void SpaceHandle(InputAction.CallbackContext context) => Space?.Invoke();
    private void AimingHandle(InputAction.CallbackContext context) => Aiming?.Invoke(context.ReadValue<Vector2>());
    private void ResetAimingHandle(InputAction.CallbackContext context) => Aiming?.Invoke(Vector2.zero);
    private void PullStartedHandle(InputAction.CallbackContext context) => SetPull?.Invoke(true);
    private void PullCanceledHandle(InputAction.CallbackContext context) => SetPull?.Invoke(false);
    private void SetAimStartedHandle(InputAction.CallbackContext context) => SetAim?.Invoke(true);
    private void SetAimCanceledHandle(InputAction.CallbackContext context) => SetAim?.Invoke(false);

}