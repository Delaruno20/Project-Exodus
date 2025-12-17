public interface IActivitySequence
{
    bool IsCompleted { get; }
    void Start();
    bool Update();
}