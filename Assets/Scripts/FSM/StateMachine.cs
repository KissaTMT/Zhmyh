using System;
using System.Collections.Generic;
using System.Linq;

public class StateMachine
{
    public State CurrentState => _currentState;
    private State _currentState;

    private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();
    private List<Transition> _currentTransitions = new List<Transition>();
    private List<Transition> _anyTransitions = new List<Transition>();
    private static List<Transition> empty = new List<Transition>(0);

    public void Update()
    {
        var transition = GetTransition();
        if (transition != null) SetState(transition.To);

        _currentState?.Update();
    }
    public void SetState(State state)
    {
        if (state == _currentState) return;

        if (_currentState is DecorateState decorate)
        {
            decorate.BaseState.Exit();
            if (_transitions[_currentState.GetType()].FirstOrDefault(i => state == i.To) != null)
            {
                decorate.SetBaseState(state);
                state.Enter();
            }
        }

        if (state is DecorateState)
        {
            var formerState = _currentState;
            _currentState = state;
            ((DecorateState)_currentState).SetBaseState(formerState);
        }
        else
        {
            _currentState?.Exit();
            _currentState = state;
        }

        if (!_transitions.TryGetValue(_currentState.GetType(), out _currentTransitions)) _currentTransitions = empty;

        _currentState.Enter();
    }
    public void AddTransition(State from, State to, Func<bool> predicate)
    {
        if (!_transitions.TryGetValue(from.GetType(), out var transitions))
        {
            transitions = new List<Transition>();
            _transitions[from.GetType()] = transitions;
        }

        transitions.Add(new Transition(to, predicate));
    }
    public void AddTransition(State from, DecorateState to, Func<bool> predicate, Action runOverride)
    {
        AddTransition(from, to, predicate);
        to.AddOverride(from, runOverride);
    }
    public void AddAnyTransition(State to, Func<bool> predicate) => _anyTransitions.Add(new Transition(to, predicate));
    private Transition GetTransition()
    {
        if(_currentState is DecorateState decorate)
        {
            var baseTransition = _transitions[decorate.BaseState.GetType()];
            for (var i = 0; i < baseTransition.Count; i++)
            {
                if (baseTransition[i].Condition()) return baseTransition[i];
            }
        }
        for (var i = 0; i < _anyTransitions.Count; i++)
        {
            if (_anyTransitions[i].Condition()) return _anyTransitions[i];
        }
        for(var i = 0; i < _currentTransitions.Count; i++)
        {
            if (_currentTransitions[i].Condition()) return _currentTransitions[i];
        }
        return null;
    }
    private class Transition
    {
        public Func<bool> Condition { get; }
        public State To { get; }

        public Transition(State to, Func<bool> condition)
        {
            To = to;
            Condition = condition;
        }
    }
}