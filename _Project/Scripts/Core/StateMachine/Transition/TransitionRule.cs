public class TransitionRule
{
    public ITransitionCondition Condition;
    public IState TargetState;

    public TransitionRule(ITransitionCondition condition,  IState targetState)
    {
        Condition = condition;
        TargetState = targetState;
    }
}