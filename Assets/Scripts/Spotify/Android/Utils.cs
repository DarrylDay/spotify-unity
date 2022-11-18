#if UNITY_ANDROID

using System;
using UnityEngine;

namespace Spotify.Android
{
    public class ConnectionListener : AndroidJavaProxy
    {
        private Action<AndroidJavaObject> _onConnected;
        private Action<string> _onFailure;

        public ConnectionListener(Action<AndroidJavaObject> onConnected, Action<string> onFailure) :
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

    public class AuthCallback : AndroidJavaProxy
    {
        private readonly Action<AndroidJavaObject> _instanceCallback;
        private readonly Action<string, string> _resultCallback;
        private readonly Action<string> _errorCallback;

        public AuthCallback(Action<AndroidJavaObject> instanceCallback, Action<string, string> resultCallback, Action<string> errorCallback = null) 
            : base(Application.identifier + ".Utils$AuthCallback")
        {
            _instanceCallback = instanceCallback;
            _resultCallback = resultCallback;
            _errorCallback = errorCallback;
        }

        public void onInstance(AndroidJavaObject instance)
        {
            _instanceCallback?.Invoke(instance);
        }
        
        public void onResult(string type, string token)
        {
            _resultCallback?.Invoke(type, token);
        }

        public void onError(string error)
        {
            _errorCallback?.Invoke(error);
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

#endif