using System;
using UnityEngine;

public class ConnectionListner : AndroidJavaProxy
{
    private Action<AndroidJavaObject> _onConnected;
    private Action<string> _onFailure;

    public ConnectionListner(Action<AndroidJavaObject> onConnected, Action<string> onFailure) :
        base("com.spotify.android.appremote.api.Connector$ConnectionListener")
    {
        _onConnected = onConnected;
        _onFailure = onFailure;
    }

    public void onConnected(AndroidJavaObject spotifyAppRemote)
    {
        _onConnected?.Invoke(spotifyAppRemote);
    }

    public void onFailure(AndroidJavaObject throwable)
    {
        _onFailure?.Invoke(throwable.Call<string>("getMessage"));
        throwable.Dispose();
    }
}
