using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CallResult<T> : ICallResult<T>, IDisposable
{
    public ResultState State { get; private set; } = ResultState.Pending;

    protected Action<T> _onResult;
    protected Action<Exception> _onError;

    protected T _result;
    protected Exception _error;

    protected TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();
    protected Coroutine _callCoroutine;
    protected CancellationTokenSource _callCTS;
    protected Action _cancelAction;

    public CallResult() { }

    public CallResult(Func<CallResult<T>, IEnumerator> getEnumerator)
    {
        _callCoroutine = MonoBehaviourHelper.RunCoroutine(getEnumerator.Invoke(this));
    }

    public CallResult(Func<CancellationTokenSource, Task<T>> runTask)
    {
        _callCTS = new CancellationTokenSource();
        MonoBehaviourHelper.OnApplicationQuitEvent += Dispose;
        RunAsyncTask(runTask(_callCTS));
    }

    public CallResult(T result)
    {
        _result = result;
        State = ResultState.Finished;
    }

    public ICallResult<T> OnResult(Action<T> callback)
    {
        _onResult = callback;
        if (State == ResultState.Finished) _onResult?.Invoke(_result);
        return this;
    }

    public ICallResult<T> OnFinish(Action callback)
    {
        _onResult = (t) => callback?.Invoke();
        if (State == ResultState.Finished) _onResult?.Invoke(_result);
        return this;
    }

    public ICallResult<T> OnError(Action<Exception> callback)
    {
        _onError = callback;
        if (State == ResultState.Error) _onError?.Invoke(_error);
        return this;
    }

    public bool TryCancel()
    {
        if (State == ResultState.Canceled) return true;

        if (_callCoroutine != null)
        {
            MonoBehaviourHelper.AbortCoroutine(_callCoroutine);
            State = ResultState.Canceled;
        }
        else if (_callCTS != null)
        {
            _callCTS?.Cancel();
            _callCTS?.Dispose();
            State = ResultState.Canceled;
        }
        else if (_cancelAction != null)
        {
            _cancelAction?.Invoke();
            State = ResultState.Canceled;
        }

        return State == ResultState.Canceled;
    }

    public bool IsCanceled()
    {
        return State == ResultState.Canceled;
    }

    public bool IsCancelable()
    {
        return _callCoroutine != null || _callCTS != null || _cancelAction != null;
    }

    // TODO Add timeout option

    public async Task Await()
    {
        if (State == ResultState.Pending)
        {
            await _completionSource.Task;
        }

        return;
    }

    public async Task<T> AwaitResult()
    {
        if (State == ResultState.Finished)
        {
            return _result;
        }

        return await _completionSource.Task;
    }

    public IEnumerator Yield()
    {
        if (State == ResultState.Finished)
        {
            yield break;
        }

        while (State == ResultState.Pending)
            yield return null;
    }

    public void SetResult(T result)
    {
        if (State != ResultState.Pending)
        {
            throw new Exception("Cannot Set Result on Non-Pending State");
        }
        else
        {
            _result = result;
            State = ResultState.Finished;
            _onResult?.Invoke(result);
        }
    }

    public void SetError(Exception e)
    {
        if (State != ResultState.Pending)
        {
            throw new Exception("Cannot Set Error on Non-Pending State");
        }
        else
        {
            _error = e;
            State = ResultState.Error;
            _onError?.Invoke(e);
        }
    }

    public ResultState GetState() => State;

    public T GetResult()
    {
        switch (State)
        {
            case ResultState.Pending:
                throw new Exception("Result Pending");
            case ResultState.Canceled:
                throw new Exception("Call Canceled");
            case ResultState.Error:
                if (_error != null) throw _error;
                else throw new Exception("Unknown Error");
            case ResultState.Finished:
                return _result;
            default:
                throw new Exception("Unknown Error");
        }
    }

    public void Dispose()
    {
        if (State == ResultState.Pending) TryCancel();
    }

    private async Task RunAsyncTask(Task<T> task)
    {
        try
        {
            SetResult(await task);
        }
        catch (Exception e)
        {
            SetError(e);
        }

        MonoBehaviourHelper.OnApplicationQuitEvent -= Dispose;
    }
}

public class CallResult : CallResult<Empty>, ICallResult
{
    public readonly static Empty Empty = new Empty();

    public CallResult() : base() { }

    public CallResult(Func<CallResult, IEnumerator> getEnumerator)
    {
        _callCoroutine = MonoBehaviourHelper.RunCoroutine(getEnumerator.Invoke(this));
    }

    public CallResult(Func<CancellationTokenSource, Task> runTask)
    {
        _callCTS = new CancellationTokenSource();
        MonoBehaviourHelper.OnApplicationQuitEvent += Dispose;
        RunAsyncTask(runTask(_callCTS));
    }

    public void SetFinished()
    {
        SetResult(Empty);
    }

    public new ICallResult OnFinish(Action callback)
    {
        _onResult = (t) => callback?.Invoke();
        if (State == ResultState.Finished) _onResult?.Invoke(_result);
        return this;
    }

    public new ICallResult OnError(Action<Exception> callback)
    {
        _onError = callback;
        if (State == ResultState.Error) _onError?.Invoke(_error);
        return this;
    }

    private async Task RunAsyncTask(Task task)
    {
        try
        {
            await task;
            SetResult(CallResult.Empty);
        }
        catch (Exception e)
        {
            SetError(e);
        }

        MonoBehaviourHelper.OnApplicationQuitEvent -= Dispose;
    }
}