using System.Threading;
using System.Threading.Tasks;

public interface IActivity
{
    ActivityState State { get; }
    Task ActivateAsync(CancellationToken cancellationToken);
    Task DeactivateAsync(CancellationToken cancellationToken);
}

public enum ActivityState
{
    Inactive,
    Activating,
    Active,
    Deactivating,
    Faulted
}