using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Spotify
{
    public class Config : ScriptableObject
    {

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Spotify/Create Config File")]
        public static void Create()
        {
            var config = CreateInstance<Config>();
            config.ANDROID_AUTH_REDIRECT_URI = "http://" + Application.identifier + "/callback";
            config.SCOPES = ALL_SCOPES.ToArray();
            UnityEditor.AssetDatabase.CreateAsset(config, "Assets/Resources/" + AssetName + ".asset");
        }
#endif
        public static readonly string AssetName = "SpotifyConfig";

        public static Config Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<Config>(AssetName);

#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        Create();
                        _instance = Resources.Load<Config>(AssetName);
                    }
#endif
                }

                return _instance;
            }
        }
        private static Config _instance;

        public static readonly IReadOnlyList<string> ALL_SCOPES = new List<string>()
        {
            "ugc-image-upload",
            "user-read-playback-state",
            "user-modify-playback-state",
            "user-read-currently-playing",
            "app-remote-control",
            "streaming",
            "playlist-read-private",
            "playlist-read-collaborative",
            "playlist-modify-private",
            "playlist-modify-public",
            "user-follow-modify",
            "user-follow-read",
            "user-read-playback-position",
            "user-top-read",
            "user-read-recently-played",
            "user-library-modify",
            "user-library-read",
            "user-read-email",
            "user-read-private"
        };

        public string CLIENT_ID;
        public string DESKTOP_AUTH_REDIRECT_URI = "http://localhost:51772/callback";
        public string ANDROID_AUTH_REDIRECT_URI;
        public string[] SCOPES;

        public string TOKEN_ENDPOINT = "https://accounts.spotify.com/api/token";
        public string AUTH_ENDPOINT = "https://accounts.spotify.com/authorize";
        public string API_ENDPOINT = "https://api.spotify.com/v1";

        public static PlayerPrefString CLIENT_SECRET_PP = new PlayerPrefString("SPOTIFY_CLIENT_SECRET");
        public string CLIENT_SECRET => CLIENT_SECRET_PP.Read();

#if UNITY_EDITOR
        public void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
    }
}