using System;
using UnityEngine;

public class InputHandler : IDisposable, IInput
{
    public event Action<Vector2> OnDirection;
    public event Action<Vector2> OnAim;
    public event Action OnSpace;
    public event Action OnShoot;
    public event Action OnSetAim;

    private InputActions _inputs;
    private float _sensivity = 4;

    public InputHandler()
    {
        _inputs = new InputActions();
        _inputs.Enable();

        _inputs.Gameplay.Direction.performed += movement => OnDirection?.Invoke(movement.ReadValue<Vector2>());
        _inputs.Gameplay.Direction.canceled += movement => OnDirection?.Invoke(Vector2.zero);

        _inputs.Gameplay.Aim.performed += aim => OnAim?.Invoke(aim.ReadValue<Vector2>()* _sensivity);
        _inputs.Gameplay.Aim.canceled += aim => OnAim?.Invoke(Vector2.zero);

        _inputs.Gameplay.Space.performed += space => OnSpace?.Invoke();
        _inputs.Gameplay.Shoot.performed += shoot => OnShoot?.Invoke();
        _inputs.Gameplay.SetAim.performed += setAim => OnSetAim?.Invoke();
    }
    public void Dispose()
    {
        _inputs.Gameplay.Direction.performed -= movement => OnDirection?.Invoke(movement.ReadValue<Vector2>());
        _inputs.Gameplay.Direction.canceled -= movement => OnDirection?.Invoke(Vector2.zero);

        _inputs.Gameplay.Aim.performed -= aim => OnAim?.Invoke(aim.ReadValue<Vector2>());
        _inputs.Gameplay.Aim.canceled -= aim => OnAim?.Invoke(Vector2.zero);

        _inputs.Gameplay.Space.performed -= space => OnSpace?.Invoke();
        _inputs.Gameplay.Shoot.performed -= shoot => OnShoot?.Invoke();
        _inputs.Gameplay.SetAim.performed -= setAim => OnSetAim?.Invoke();

        _inputs.Disable();
    }
}
