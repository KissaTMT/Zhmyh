using System;
using UnityEngine;

public class Timeflow
{
    public event Action<int> OnInverse;
    public int Absolute => _absolute;
    public int Relative => _relative;

    private int _absolute;
    private int _relative;

    private Timeflow _relativeFlow;

    public Timeflow(int absolute = 1) : this(absolute, 1) { }
    public Timeflow(int absolute, int relative) 
    {
        _absolute = absolute == 0 ? 1 : (int)Mathf.Sign(absolute);
        _relative = relative == 0 ? 1 : (int)Mathf.Sign(relative);
    }
    public Timeflow SetRelativeFlow(Timeflow relative)
    {
        _relativeFlow = relative;
        return this;
    }

    public Timeflow CalculateRelativeTimeflow()
    {
        _relative = (int)(_relativeFlow.Absolute == 0 ? 1 : Mathf.Sign(_relativeFlow.Absolute)) * _absolute;
        return this;
    }
    public void Inverse()
    {
        _absolute = -1 * _absolute;
        CalculateRelativeTimeflow();
        OnInverse?.Invoke(_absolute);
    }
}
