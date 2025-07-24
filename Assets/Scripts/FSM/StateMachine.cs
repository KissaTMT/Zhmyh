using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        if (_currentState is DecorateState decorate1)
        {
            var baseState = decorate1.BaseState;

            if (_transitions.TryGetValue(baseState.GetType(), out var baseTransitions))
            {
                foreach (var transition in baseTransitions)
                {
                    if (transition.To == state)
                    {
                        baseState.Exit();
                        decorate1.SetBaseState(state);
                        state.Enter();
                        return;
                    }
                }
            }
            _currentState.Exit();
            _currentState = state;
        }
        else if (state is DecorateState decorate2)
        {
            decorate2.SetBaseState(_currentState);
            _currentState = decorate2;
        }
        else
        {
            _currentState?.Exit();
            _currentState = state;
        }

        if (!_transitions.TryGetValue(_currentState.GetType(), out _currentTransitions))
            _currentTransitions = empty;

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
        foreach (var transition in _anyTransitions)
        {
            if (transition.Condition()) return transition;
        }

        foreach (var transition in _currentTransitions)
        {
            if (transition.Condition()) return transition;
        }

        if (_currentState is DecorateState decorate)
        {
            if (_transitions.TryGetValue(decorate.BaseState.GetType(), out var baseTransitions))
            {
                foreach (var transition in baseTransitions)
                {
                    if (transition.Condition()) return transition;
                }
            }
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