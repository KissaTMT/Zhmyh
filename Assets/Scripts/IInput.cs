using System;
using UnityEngine;

public interface IInput
{
    public event Action<Vector2> OnMovement;
    public event Action<Vector2> OnAim;
    public event Action OnJump;
}