using System.Collections.Generic;
using System.Threading;

public class TransitionManager  : ITransitionManager
{

    #region Fields

    private readonly HierarchicalStateMachine _stateMachine;
    private ITransitionSequence _currentSequence;
    private readonly Queue<TransitionRequest> _pendingTransitions  = new();
    private CancellationTokenSource _cancellationTokenSource;

    #endregion

    #region Properties

    public bool IsTransitioning => _currentSequence != null;

    #endregion
    
    #region Constructors

    public TransitionManager(HierarchicalStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    #endregion

    #region Public Methods

    public void RequestTransition(IState fromState, IState toState)
    {
        if (!IsValidTransition(fromState, toState))
        {
            return;
        }

        if (IsTransitioning)
        {
            _pendingTransitions.Enqueue(new TransitionRequest(fromState, toState));
            return;
        }
        
        StartTransition(fromState, toState);
    }

    public void Update()
    {
        if (IsTransitioning)
        {
            if (_currentSequence.Update())
            {
                CompleteTransition();
                return;
            }
        }
        _stateMachine?.Update();
    }

    #endregion
    
    #region Private Methods

    private bool IsValidTransition(IState fromState, IState toState)
    {
        return fromState != null && toState != null &&  fromState.Equals(toState);
    }

    private void StartTransition(IState fromState, IState toState)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var commonAncestor = StateUtility.FindLowestCommonAncestor(fromState, toState);
        var exitChain = StateUtility.GetExitChain(fromState, commonAncestor);
        var enterChain = StateUtility.GetEnterChain(toState, commonAncestor);
        
        _currentSequence = new TransitionSequence(_stateMachine, fromState, toState, exitChain, enterChain, _cancellationTokenSource.Token);
        _currentSequence.Start();
    }

    private void CompleteTransition()
    {
        _currentSequence = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        if (_pendingTransitions.Count > 0)
        {
            var request = _pendingTransitions.Dequeue();
            StartTransition(request.FromState, request.ToState);
        }
    }
    
    #endregion
}