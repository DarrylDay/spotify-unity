using System;
using UnityEngine;
using Spotify.Auth;

namespace Spotify.Android
{
	public class AndroidAuth
	{
        private void GetAuthToken(Action<string> callback)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject unityPlayerActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass spotifyLoginActivityClass = new AndroidJavaClass(Application.identifier + ".SpotifyLoginActivity"))
            {
                spotifyLoginActivityClass.CallStatic("SetTokenCallback", new TokenCallback(token =>
                {
                    Debug.Log(token);
                    callback?.Invoke(token);
                }));

                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    using (var spotifyLoginIntent = intent.Call<AndroidJavaObject>("setClassName", unityPlayerActivity, Application.identifier + ".SpotifyLoginActivity"))
                    {
                        unityPlayer.Call("startActivity", spotifyLoginIntent);
                    }
                }
            }
        }
    }
}
