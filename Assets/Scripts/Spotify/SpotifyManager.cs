using UnityEngine;
using Spotify.Auth;
using Spotify.WebAPI;

#if UNITY_ANDROID
using Spotify.Android;
#endif

namespace Spotify
{
	public class SpotifyManager : Singleton<SpotifyManager>
	{
        [SerializeField] private AuthFrontend _authFrontend;
		[SerializeField] private PlayerFrontend _playerFrontend;

        private PlayerBackend _backend;

        void Start()
        {
            if (OAuth.Authorized)
            {
                OAuth.Login()
                    .OnResult(InitPlayer)
                    .OnError(e =>
                    {
                        Debug.LogException(e);
                        _authFrontend.Setup(InitPlayer);
                    });
            }
            else
            {
                _authFrontend.Setup(InitPlayer);
            }
        }

        private void InitPlayer(OAuth.TokenHandler tokenHandler)
        {
#if UNITY_EDITOR
            _backend = new WebAPIPlayer();
#elif UNITY_ANDROID
            _backend = new AndroidPlayer();
#endif
            _backend.Init()
                .OnFinish(() =>
                {
                    _playerFrontend.Init(_backend);
                    
                    // Test
                    // WebAPI.WebAPI.GetUserPlaylists()
                    //     .OnResult(playlists =>
                    //     {
                    //         playlists.ForEach(x => Debug.Log(x.name));
                    //     })
                    //     .OnError(error =>
                    //     {
                    //         Debug.LogException(error);
                    //     });
                })
                .OnError(e =>
                {
                    // TODO: Add UI error overlay?
                    Debug.LogException(e);
                });
        }

        void OnApplicationQuit()
        {
            _backend?.Dispose();
        }
    }
}