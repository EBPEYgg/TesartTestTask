namespace TesartTestTask.Application.Services;

public class AsyncManualResetEvent
{
    private readonly object _syncRoot = new();

    private TaskCompletionSource _signal = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public AsyncManualResetEvent(bool signaled)
    {
        if (signaled)
            _signal.SetResult();
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        lock (_syncRoot)
            return _signal.Task.WaitAsync(cancellationToken);
    }

    public void Set()
    {
        lock (_syncRoot)
            _signal.TrySetResult();
    }

    public void Reset()
    {
        lock (_syncRoot)
            if (_signal.Task.IsCompleted)
                _signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}