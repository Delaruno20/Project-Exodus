using System.Collections.Generic;

public interface IState
{
    HierarchicalStateMachine StateMachine { get; }
    IState ParentState { get; }
    IState ChildState { get; set; }
    IReadOnlyList<IActivity> Activities { get; }
    void Enter();
    void Exit();
    void Update();
    void FixedUpdate();
    void LateUpdate();
    IState GetInitialChild();
    IState GetTransitionState();
}