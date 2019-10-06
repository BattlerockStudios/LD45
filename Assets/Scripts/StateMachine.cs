using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    private readonly Dictionary<string, object> m_blackboardValues = new Dictionary<string, object>();
    private readonly Dictionary<string, IState> m_states = new Dictionary<string, IState>();
    private IState m_currentState = null;

    public void AddState(IState state)
    {
        m_states[state.Name] = state;
        state.Initialize(m_blackboardValues);
    }

    public void Start(string state)
    {
        SetState(m_states[state]);
    }

    public void SetBlackboardValue(string key, object value)
    {
        m_blackboardValues[key] = value;
    }

    public void Update()
    {
        if (m_states.Count == 0)
        {
            return;
        }

        m_currentState?.Update();
        var targetState = m_currentState?.FindSatisfiedTransition();
        if (targetState != null)
        {
            SetState(m_states[targetState]);
        }
    }

    private void SetState(IState state)
    {
        var oldState = m_currentState;
        Debug.Log($"Setting state from \"{oldState?.Name}\" to \"{state?.Name}\"");
        m_currentState = state;
        Debug.Log($"State is now \"{state?.Name}\"");

        // ZAS: Send Events
        oldState?.Exit();
        state?.Enter();
    }

}

public interface IState
{
    void Initialize(Dictionary<string, object> bloackboardValues);
    string Name { get; }
    void Enter();
    void Exit();
    void Update();
    string FindSatisfiedTransition();
}

public abstract class AbstractState : IState
{

    public AbstractState(string name)
    {
        Name = name;
    }

    public void Initialize(Dictionary<string, object> blackboardValues)
    {
        m_blackboardValues = blackboardValues;
    }

    protected Dictionary<string, object> m_blackboardValues = null;

    public string Name { get; private set; }
    public DateTime? EnterTime = null;

    private string m_exitState = null;

    void IState.Enter()
    {
        EnterTime = DateTime.UtcNow;
        OnEnter();
    }

    void IState.Exit()
    {
        OnExit();
        EnterTime = null;
        m_exitState = null;
    }

    void IState.Update()
    {
        OnUpdate();
    }

    string IState.FindSatisfiedTransition()
    {
        return GetTransition();
    }

    protected void ExitToState(string stateName)
    {
        m_exitState = stateName;
    }

    protected abstract void OnEnter();
    protected abstract void OnExit();
    protected abstract void OnUpdate();

    protected virtual string GetTransition()
    {
        return m_exitState;
    }

}