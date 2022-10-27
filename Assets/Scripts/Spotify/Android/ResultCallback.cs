using System;
using UnityEngine;

public class ResultCallback : AndroidJavaProxy
{
    private Action<AndroidJavaObject> _callback;

    public ResultCallback(Action<AndroidJavaObject> callback) : base("com.spotify.protocol.client.CallResult$ResultCallback")
    {
        _callback = callback;
    }

    public void onResult(AndroidJavaObject data)
    {
        _callback?.Invoke(data);
    }
}

