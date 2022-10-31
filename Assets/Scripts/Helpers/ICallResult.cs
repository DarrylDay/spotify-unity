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

    bool TryCancel();
    bool IsCanceled();
    bool IsCancelable();

    Task Await();
    Task<T> AwaitResult();
    IEnumerator Yield();
}

public interface ICallResult : ICallResult<Empty>
{
    new ICallResult OnFinish(Action callback);
    new ICallResult OnError(Action<Exception> callback);
}
