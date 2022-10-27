using System;
using UnityEngine;

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

