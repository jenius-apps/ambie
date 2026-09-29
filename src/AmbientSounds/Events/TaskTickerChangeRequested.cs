namespace AmbientSounds.Events;

public class TaskTickerChangeRequested(
    TaskTickerChangeType changeType,
    string? newText,
    int? index = null)
{
    public TaskTickerChangeType ChangeType { get; } = changeType;

    public int? Index { get; } = index;

    public string? NewText { get; } = newText;
}

public enum TaskTickerChangeType
{
    Add,
    Edit,
}
