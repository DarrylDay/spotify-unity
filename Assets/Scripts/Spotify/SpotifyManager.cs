using UnityEngine;
using System.Collections;
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
#if UNITY_EDITOR

            OAuth.Login()
                .OnResult(tokenHandler =>
                {
                    _backend = new WebAPIPlayer(tokenHandler);
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

            return;

#endif

#if UNITY_ANDROID

            _backend = new AndroidPlayer();
            _backend.Init()
                .OnFinish(() =>
                {
                    _frontend.Init(_backend);
                })
                .OnError(e =>
                {
                    Debug.LogException(e);
                });

#endif
        }

        void OnApplicationQuit()
        {
            _backend?.Dispose();
        }
    }
}