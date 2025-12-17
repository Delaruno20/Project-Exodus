using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ParallelActivitySequence : IActivitySequence
{
    #region Fields
    
    private readonly List<IActivity> _activities;
    private readonly ActivityOperation _operation;
    private readonly CancellationToken _cancellationToken;
    private readonly List<Task> _runningTasks =  new List<Task>();
    
    #endregion

    #region Properties

    public bool IsCompleted { get; private set; }

    #endregion
    
    #region Constructors

    public ParallelActivitySequence(List<IActivity> activities, ActivityOperation operation, CancellationToken token)
    {
        _activities = activities ?? new List<IActivity>();
        _operation = operation;
        _cancellationToken = token;
    }
    
    #endregion
    
    public void Start()
    {
        foreach (var activity in _activities)
        {
            var task = _operation == ActivityOperation.Activate ? activity.ActivateAsync(_cancellationToken) : activity.DeactivateAsync(_cancellationToken);
            _runningTasks.Add(task);
        }

        if (_runningTasks.Count == 0)
        {
            IsCompleted = true;
        }
    }

    public bool Update()
    {
        if (IsCompleted) return true;
        
        bool allCompleted = true;

        foreach (var task in _runningTasks)
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[ActivitySequence] Task failed: {task.Exception}");
            }
            if (!task.IsCompleted)
            {
                allCompleted = false;
                break;
            }
        }
        IsCompleted = allCompleted;
        return allCompleted;
    }
}