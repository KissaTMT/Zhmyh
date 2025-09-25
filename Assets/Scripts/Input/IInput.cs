using System;
using UnityEngine;

public interface IInput
{
    public event Action Space;
    public event Action<bool> InitAiming;
    public event Action<bool> Pulling;

    public Vector2 GetDirection();
    public Vector2 GetAiming();
}