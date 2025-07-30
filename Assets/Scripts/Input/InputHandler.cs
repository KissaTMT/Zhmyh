using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputHandler : IDisposable, IInput
{
    private const float SENSIVITY = 100f;
    public event Action<Vector2> OnDirection;
    public event Action<Vector2> OnAim;
    public event Action<Vector2> OnAimDelta;
    public event Action OnSpace;
    public event Action OnShoot;
    public event Action<bool> OnSetAim;
    

    private Mouse _virtualMouse;
    private InputActions _inputs;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        InitializeVirtualMouse();
        InputState.Change(_virtualMouse.position, new Vector2(Screen.width/2,Screen.height/2));
        InputSystem.onAfterUpdate += Update;

        _inputs.Gameplay.Direction.performed += movement => OnDirection?.Invoke(movement.ReadValue<Vector2>());
        _inputs.Gameplay.Direction.canceled += movement => OnDirection?.Invoke(Vector2.zero);
        _inputs.Gameplay.Space.performed += space => OnSpace?.Invoke();
        _inputs.Gameplay.Shoot.canceled += shoot => OnShoot?.Invoke();
        _inputs.Gameplay.SetAim.started += setAim => OnSetAim?.Invoke(true);
        _inputs.Gameplay.SetAim.canceled += setAim => OnSetAim?.Invoke(false);
    }
    public void Dispose()
    {
        InputSystem.onAfterUpdate -= Update;

        _inputs.Gameplay.Direction.performed -= movement => OnDirection?.Invoke(movement.ReadValue<Vector2>());
        _inputs.Gameplay.Direction.canceled -= movement => OnDirection?.Invoke(Vector2.zero);
        _inputs.Gameplay.Space.performed -= space => OnSpace?.Invoke();
        _inputs.Gameplay.Shoot.canceled -= shoot => OnShoot?.Invoke();
        _inputs.Gameplay.SetAim.started -= setAim => OnSetAim?.Invoke(true);
        _inputs.Gameplay.SetAim.canceled -= setAim => OnSetAim?.Invoke(false);

        _inputs.Disable();
        if (_virtualMouse != null) InputSystem.RemoveDevice(_virtualMouse);
    }
    private void Update()
    {
        var delta = _inputs.Gameplay.Aim.ReadValue<Vector2>();
        var position = _virtualMouse.position.ReadValue() + delta * Time.deltaTime * SENSIVITY;

        position.x = Mathf.Clamp(position.x, 0, Screen.width);
        position.y = Mathf.Clamp(position.y, 0, Screen.height);

        InputState.Change(_virtualMouse.position, position);
        InputState.Change(_virtualMouse.delta, delta);

        OnAim?.Invoke(position);
        OnAimDelta?.Invoke(delta);
    }
    private void InitializeVirtualMouse()
    {
        if (_virtualMouse == null)
        {
            _virtualMouse = InputSystem.AddDevice<Mouse>("VirtualMouse");
        }
        else if (!_virtualMouse.added)
        {
            InputSystem.AddDevice(_virtualMouse);
        }
    }
}