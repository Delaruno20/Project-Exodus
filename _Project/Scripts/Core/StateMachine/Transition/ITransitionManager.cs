public interface ITransitionManager
{
    bool IsTransitioning { get; }
    void RequestTransition(IState fromState, IState toState);
    void Update();
}