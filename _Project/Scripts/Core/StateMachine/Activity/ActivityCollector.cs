using System.Collections.Generic;

public static class ActivityCollector
{
    public static List<IActivity> CollectActiveActivities(List<IState> states)
    {
        List<IActivity> activities = new List<IActivity>();

        foreach (var state in states)
        {
            foreach (var activity in state.Activities)
            {
                if (activity.State == ActivityState.Active)
                {
                    activities.Add(activity);
                }
            }
        }
        return activities;
    }
}