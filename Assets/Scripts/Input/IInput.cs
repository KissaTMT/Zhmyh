using System;
using UnityEngine;

public interface IInput
{
    public event Action<Vector2> OnDirection;
    public event Action<Vector2> OnAim;
    public event Action<bool> OnSetAim;
    public event Action OnSpace;
    public event Action OnShoot;
}