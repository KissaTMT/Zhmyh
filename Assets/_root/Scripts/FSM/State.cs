using System;

public abstract class State
{
    public Action Ticker;
    public void Tick() => Ticker();
    public virtual void Enter()
    {
        ResetTicker();
        OnEnter();
    }
    public virtual void Exit() => OnExit();
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnTick() { }
    protected void ResetTicker() => Ticker = OnTick;
}