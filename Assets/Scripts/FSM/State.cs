using System;

public abstract class State
{
    public Action Run;
    public virtual void Update() => Run?.Invoke();
    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void ReloadRun() { }
}