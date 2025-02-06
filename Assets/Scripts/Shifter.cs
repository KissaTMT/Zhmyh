using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shifter
{
    private Transform _root;
    public Vector2 _facingDirection;
    public Shifter(Transform root)
    {
        _root = root;
    }
    public void Shift(Vector2 facingDirection)
    {
        _facingDirection = facingDirection;
    }
    public void Attach(Transform transform, Action action)
    {

    }
    public void Detach(Transform transform)
    {

    }
}