using System;
using System.Collections.Generic;

public abstract class DecorateState : State
{
    public State BaseState => baseState;

    protected State baseState;

    private Dictionary<State, Action> _overrides = new Dictionary<State, Action>();

    public void SetBaseState(State baseState) => this.baseState = baseState;
    public void AddOverride(State baseState, Action runOverride) => _overrides[baseState] = runOverride;
    public void SetOverride() => baseState.Run = _overrides.ContainsKey(baseState) ? _overrides[baseState] : baseState.Run;
    public override void Enter() => baseState.Run = SetOverride;
    public override void Exit() => baseState.Run = baseState.ReloadRun;
    public override void Update()
    {
        base.Update();
        baseState.Update();
    }
}