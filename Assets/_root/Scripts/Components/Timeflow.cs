using System;
using UnityEngine;

public class Timeflow
{
    public event Action<int> OnInverse;
    public int Current => _isPositiveTimeflow ? 1 : -1;
    private bool _isPositiveTimeflow = true;
    public Timeflow(bool isPositiveTimeflow = true) => _isPositiveTimeflow = isPositiveTimeflow;
    public void Inverse()
    {
        _isPositiveTimeflow = !_isPositiveTimeflow;
        OnInverse?.Invoke(Current);
    }
}
