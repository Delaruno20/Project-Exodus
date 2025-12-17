using System.Collections.Generic;
using System.Threading;

public class TransitionSequence : ITransitionSequence
{
    #region Fields

    private readonly HierarchicalStateMachine _stateMachine;
    private readonly IState _fromState;
    private readonly IState _toState;
    private readonly List<IState> _enterChain;
    private readonly List<IState> _exitChain;
    private readonly CancellationToken _cancellationToken;
    
    private TransitionPhase _currentPhase;
    private IActivitySequence _activitySequence;

    #endregion

    #region Properties

    public bool IsCompleted { get; private set; }

    #endregion
    
    #region Constructors

    public TransitionSequence(HierarchicalStateMachine stateMachine, IState fromState, IState toState,
        List<IState> enterChain, List<IState> exitChain, CancellationToken token)
    {
        _stateMachine = stateMachine;
        _fromState = fromState;
        _toState = toState;
        _enterChain = enterChain;
        _exitChain = exitChain;
        _cancellationToken = token;
    }
    
    #endregion

    #region Public Methods

    public void Start()
    {
        _currentPhase = TransitionPhase.DeactivatingActivities;
        StartDeactivationPhase();
    }

    public bool Update()
    {
        if (IsCompleted) return true;

        switch (_currentPhase)
        {
            case TransitionPhase.DeactivatingActivities:
                if (_activitySequence.Update())
                {
                    StartStateChangePhase();
                }
                break;
            
            case TransitionPhase.ActivatingActivities:
                if (_activitySequence.Update())
                {
                    IsCompleted =  true;
                }
                break;
            
            case TransitionPhase.ChangingStates:
                StartActivationPhase();
                break;
        }
        
        return IsCompleted;
    }

    #endregion
    
    #region Private Methods

    private void StartDeactivationPhase()
    {
        var activities = ActivityCollector.CollectActiveActivities(_exitChain);
        _activitySequence = new ParallelActivitySequence(activities, ActivityOperation.Deactivate, _cancellationToken);
        _activitySequence.Start();
    }

    private void StartActivationPhase()
    {
        var activities = ActivityCollector.CollectActiveActivities(_enterChain);
        _activitySequence = new ParallelActivitySequence(activities, ActivityOperation.Activate, _cancellationToken);
        _activitySequence.Start();
    }

    private void StartStateChangePhase()
    {
        _currentPhase = TransitionPhase.ChangingStates;
        _stateMachine.PerformTransition(_fromState, _toState);
    }
    
    #endregion
}