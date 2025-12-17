using System;
using System.Collections.Generic;
using UnityEngine;

public class HierarchicalStateMachine
{
    #region  Properties

    private IState RootState { get; set; }
    public ITransitionManager TransitionManager { get; private set; }
    public bool IsStarted { get; private set; }

    #endregion

    #region Constructor

    public HierarchicalStateMachine(IState rootState)
    {
        //SetRootState(rootState);
        TransitionManager = new TransitionManager(this);
    }

    #endregion

    #region Public Methods

    public void SetRootState(IState rootState)
    {
        RootState = rootState ?? throw new ArgumentException(nameof(rootState));
    }

    public void Start()
    {
        if (IsStarted) return;
        IsStarted = true;
        RootState?.Enter();
    }

    public void Update()
    {
        if (!IsStarted) Start();
        RootState?.Update();
    }

    public void FixedUpdate()
    {
        if (!IsStarted) Start();
        RootState?.FixedUpdate();
    }
    
    public void LateUpdate()
    {
        if(!IsStarted)
            Start();
        RootState?.LateUpdate();
    }

    public void PerformTransition(IState fromState, IState toState)
    {
        if (!IsValidTransition(fromState, toState))
        {
            return;
        }
        var commonAncestor = StateUtility.FindLowestCommonAncestor(fromState, toState); 
        
        ExitStatesUpTo(fromState, commonAncestor);
        EnterStatesDownTo(toState, commonAncestor);
    }
    
    public string GetCurrentStatePath()
    {
        IState currentState = RootState;
        var path = $"{RootState}";
        /*
        for (IState currentState = RootState; currentState?.ChildState != null; currentState = currentState?.ChildState)
        {
            path += $"->{currentState}";
        }
        */

        while (currentState.ChildState != null)
        {
            path += $" -> {currentState.ChildState}";
            currentState = currentState.ChildState;
        }

        return path;
    }

    public void Dispose()
    {
        RootState =  null;
        TransitionManager = null;
        IsStarted = false;
    }

    #endregion
    
    #region Private Methods

    private bool IsValidTransition(IState fromState, IState toState)
    {
        return fromState != null && toState != null && !fromState.Equals(toState);
    }

    private void ExitStatesUpTo(IState fromState, IState ancestor)
    {
        for (IState currentState = fromState; currentState != ancestor; currentState = currentState?.ParentState)
        {
            currentState?.Exit();
        }
    }

    private void EnterStatesDownTo(IState toState, IState ancestor)
    {
        Stack<IState> stateToEnter = new Stack<IState>();

        for (IState currentState = toState; currentState != ancestor; currentState = currentState?.ParentState)
        {
            stateToEnter.Push(currentState);
        }

        while (stateToEnter.Count > 0)
        {
            stateToEnter.Pop().Enter();
        }
    }
    
    #endregion
}