using System;
using UnityEngine;

public class Timeflow
{
    public event Action<int> OnInverse;
    public int Current => _current;

    private int _current;
    public Timeflow(int current = 1) => _current = (int)Mathf.Sign(current);
    public void Inverse()
    {
        _current = -1 * _current;
         OnInverse?.Invoke(_current);
    }
}
