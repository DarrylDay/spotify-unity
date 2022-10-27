using UnityEngine;
using System.Collections;
using Spotify.Auth;
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

            OAuth.RefreshAccessToken((token) =>
            {
                _backend = new WebAPIPlayer(token);
                _backend.Init().OnFinish(() =>
                {
                    _frontend.Init(_backend);
                });
            });

#elif UNITY_ANDROID




#endif
        }

        void OnApplicationQuit()
        {
            _backend?.Dispose();
        }
    }
}