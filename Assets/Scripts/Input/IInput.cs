using System;
using UnityEngine;

public interface IInput
{
    public event Action Dash;
    public event Action Jump;
    public event Action CameraReset;
    public event Action<bool> Pulling;

    public Vector2 GetMove();
    public Vector2 GetLook();
}