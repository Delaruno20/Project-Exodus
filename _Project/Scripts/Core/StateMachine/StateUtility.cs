using System.Collections.Generic;

public static class StateUtility
{
    public static IState FindLowestCommonAncestor(IState state1, IState state2)
    {
        HashSet<IState> stateSet = new HashSet<IState>();
        for (IState state = state1; state != null; state = state.ParentState)
        {
            stateSet.Add(state);
        }
        IState currentState = state2;

        while (currentState != null)
        {
            if (stateSet.Contains(currentState))
            {
                return currentState;
            }
            currentState = currentState.ParentState;
        }
        return null;
    }

    public static List<IState> GetExitChain(IState from, IState ancestor)
    {
        List<IState> chains = new List<IState>();
        for (IState currentState = from; currentState != ancestor; currentState = currentState?.ParentState)
        {
            if(currentState == null)
                break;
            chains.Add(currentState);
        }
        return chains;
    }

    public static List<IState> GetEnterChain(IState to, IState ancestor)
    {
        Stack<IState> stateToEnter = new Stack<IState>();
        for (IState currentState = to; currentState != ancestor; currentState = currentState?.ParentState)
        {
            if(currentState == null)
                break;
            stateToEnter.Push(currentState);
        }
        return new List<IState>(stateToEnter);
    }
}