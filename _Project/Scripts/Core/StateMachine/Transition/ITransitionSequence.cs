public interface ITransitionSequence
{
    bool IsCompleted { get; }
    void Start();
    bool Update();
}