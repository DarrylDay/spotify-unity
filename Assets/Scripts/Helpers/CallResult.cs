using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class Empty { }

public enum ResultState
{
    Pending,
    Canceled,
    Error,
    Finished
}

public interface ICallResult<T>
{
    ResultState GetState();
    T GetResult();

    ICallResult<T> OnResult(Action<T> callback);
    ICallResult<T> OnFinish(Action callback);
    ICallResult<T> OnError(Action<Exception> callback);

    void Cancel();

    Task<T> Await();
    Coroutine Yield();
}

public interface ICallResult : ICallResult<Empty>
{
    new ICallResult OnFinish(Action callback);
    new ICallResult OnError(Action<Exception> callback);
}

public class CallResult<T> : ICallResult<T>
{
    protected ResultState _state;
    protected T _result;

    protected Action<T> _onResult;
    protected Action<Exception> _onError;

    protected Coroutine _coroutine;
    protected Task _task;

    protected Exception _error;

    public CallResult() {}

    public CallResult(Func<CallResult<T>, IEnumerator> getEnumerator)
    {
        _coroutine = CoroutineHelper.Run(getEnumerator.Invoke(this));
    }

    public CallResult(IEnumerator enumerator)
    {
        _coroutine = CoroutineHelper.Run(enumerator);
    }

    public CallResult(Coroutine coroutine)
    {
        _coroutine = coroutine;
    }

    public CallResult(Task task)
    {
        throw new NotImplementedException();
    }

    public ICallResult<T> OnResult(Action<T> callback)
    {
        _onResult = callback;
        return this;
    }

    public ICallResult<T> OnFinish(Action callback)
    {
        _onResult = (t) => callback?.Invoke();
        return this;
    }

    public ICallResult<T> OnError(Action<Exception> callback)
    {
        _onError = callback;
        return this;
    }

    public void Cancel()
    {
        throw new NotImplementedException();
    }

    public bool IsCanceled()
    {
        throw new NotImplementedException();
    }

    public Task<T> Await()
    {
        throw new NotImplementedException();
    }

    public Coroutine Yield()
    {
        if (_coroutine != null)
        {
            return _coroutine;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public void SetResult(T result)
    {
        _onResult?.Invoke(result);
    }

    public void SetError(Exception e)
    {
        _onError?.Invoke(e);
    }

    public ResultState GetState() => _state;

    public T GetResult()
    {
        switch (_state)
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
}

public class CallResult : CallResult<Empty>, ICallResult
{
    public readonly static Empty Empty = new Empty();

    public CallResult() : base() { }

    public CallResult(Func<CallResult, IEnumerator> getEnumerator)
    {
        _coroutine = CoroutineHelper.Run(getEnumerator.Invoke(this));
    }

    public new ICallResult OnFinish(Action callback)
    {
        _onResult = (t) => callback?.Invoke();
        return this;
    }

    public new ICallResult OnError(Action<Exception> callback)
    {
        _onError = callback;
        return this;
    }
}