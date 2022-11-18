using System;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Spotify.Auth;
using Spotify.Android;
using Spotify.WebAPI;

namespace Spotify
{
	public class SpotifyManager : Singleton<SpotifyManager>
	{
		[SerializeField] private PlayerFrontend _frontend;

        private PlayerBackend _backend;

        void Start()
        {
            Debug.Log("Spotify On Start");

            OAuth.Login()
                .OnResult(tokenHandler =>
                {
#if UNITY_EDITOR
                    _backend = new WebAPIPlayer(tokenHandler);
#elif UNITY_ANDROID
                    _backend = new AndroidPlayer();
#endif
                    _backend.Init()
                        .OnFinish(() =>
                        {
                            _frontend.Init(_backend);
                        })
                        .OnError(e =>
                        {
                            Debug.LogException(e);
                        });
                })
                .OnError(e =>
                {
                    Debug.LogException(e);
                });
        }

        void OnApplicationQuit()
        {
            _backend?.Dispose();
        }
    }
}