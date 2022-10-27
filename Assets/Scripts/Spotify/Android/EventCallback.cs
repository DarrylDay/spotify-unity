using System;
using UnityEngine;

public class EventCallback : AndroidJavaProxy
{
    private Action<AndroidJavaObject> _callback;

    public EventCallback(Action<AndroidJavaObject> callback) : base("com.spotify.protocol.client.Subscription$EventCallback")
    {
        _callback = callback;
    }

    public void onEvent(AndroidJavaObject data)
    {
        _callback?.Invoke(data);
    }
}
