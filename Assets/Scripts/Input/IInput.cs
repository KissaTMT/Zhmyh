using System;
using UnityEngine;

public interface IInput
{
    public event Action<Vector2> Direction;
    public event Action<Vector2> Aiming;
    public event Action Space;
    public event Action<bool> SetAim;
    public event Action<bool> SetPull;
}