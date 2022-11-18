#if UNITY_ANDROID

using System;
using UnityEngine;
using Spotify.Auth;

namespace Spotify.Android
{
	public static class AndroidAuth
	{
        public static ICallResult<OAuth.AuthCodeResponse> GetAuthCode()
        {
            var callResult = new CallResult<OAuth.AuthCodeResponse>();

            // To ensure on main thread
            var clientID = Config.Instance.CLIENT_ID;
            var redirect = Config.Instance.ANDROID_AUTH_REDIRECT_URI;
            var scopes = Config.Instance.SCOPES;
            
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var unityPlayerActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var spotifyLoginActivityClass = new AndroidJavaClass(Application.identifier + ".SpotifyLoginActivity"))
            {
                spotifyLoginActivityClass.CallStatic("SetAuthCallback", new AuthCallback(
                    (instance) =>
                    {
                        instance.Call("Authorize", clientID, redirect, scopes);
                    }, 
                    (type, token) =>
                    {
                        if (type != "CODE")
                        {
                            callResult.SetError(new Exception("Invalid Token Return Type"));
                        }
                        else
                        {
                            callResult.SetResult(new OAuth.AuthCodeResponse()
                            {
                                Code = token
                            });
                        }
                    },
                    (error) =>
                    {
                        callResult.SetError(new Exception(error));
                    }));

                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    using (var spotifyLoginIntent = intent.Call<AndroidJavaObject>("setClassName", unityPlayerActivity, Application.identifier + ".SpotifyLoginActivity"))
                    {
                        unityPlayerActivity.Call("startActivity", spotifyLoginIntent);
                    }
                }
            }

            return callResult;
        }
    }
}

#endif