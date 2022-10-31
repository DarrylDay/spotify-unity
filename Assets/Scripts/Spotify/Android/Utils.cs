using System;
using UnityEngine;


namespace Spotify.Android
{
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

    public class TokenCallback : AndroidJavaProxy
    {
        private Action<string> _callback;

        public TokenCallback(Action<string> callback) : base("ca.darrylday.spotifyandroidtest.Utils$TokenCallback")
        {
            _callback = callback;
        }

        public void onToken(string token)
        {
            _callback?.Invoke(token);
        }
    }

    public class ResultCallback<T> : AndroidJavaProxy
    {
        private Action<T> _callback;

        public ResultCallback(Action<T> callback) : base("com.spotify.protocol.client.CallResult$ResultCallback")
        {
            _callback = callback;
        }

        public void onResult(T data)
        {
            _callback?.Invoke(data);
        }
    }

    public class ErrorCallback : AndroidJavaProxy
    {
        private Action<AndroidJavaObject> _callback;

        public ErrorCallback(Action<AndroidJavaObject> callback) : base("com.spotify.protocol.client.ErrorCallback")
        {
            _callback = callback;
        }

        public void onError(AndroidJavaObject data)
        {
            _callback?.Invoke(data);
        }
    }

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

}


