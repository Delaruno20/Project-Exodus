public struct TransitionRequest
{
    public IState FromState;
    public IState ToState;

    public TransitionRequest(IState fromState, IState toState)
    {
        FromState = fromState;
        ToState = toState;
    }
}