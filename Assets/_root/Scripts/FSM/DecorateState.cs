using System;
using System.Collections.Generic;

public abstract class DecorateState : State
{
    public State BaseState { get; private set; }

    private Dictionary<State, Action> _overrides = new Dictionary<State, Action>();

    public void SetBaseState(State baseState)
    {
        if(BaseState != null) Unsubscribe();

        BaseState = baseState;

        Subscribe();
    }
    public void AddOverride(State baseState, Action tickerOverride) => _overrides[baseState] = tickerOverride;
    public override void Enter()
    {
        base.Enter();
        Subscribe();
    }
    public override void Exit()
    {
        Unsubscribe();
        base.Exit();
    }
    private Action SetOverride() => _overrides.ContainsKey(BaseState) ? _overrides[BaseState] : BaseState.Ticker;
    private void Subscribe()
    {
        BaseState.Ticker = SetOverride();
        Ticker += BaseState.Ticker;
    }
    private void Unsubscribe()
    {
        BaseState.Ticker = BaseState.OnTick;
        ResetTicker();
    }
}