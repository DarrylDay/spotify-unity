using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Spotify.Auth
{
    public static class OAuth
    {
        public class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        private static string _tokenEndpoint = "https://accounts.spotify.com/api/token";
        private static string _authorizationEndpoint = "https://accounts.spotify.com/authorize";
        private static string _responseType = "code";
        private static string _redirectUri = "http://localhost:51772/callback";
        private static string[] _scopes =
        {
            "user-read-playback-state",
            "app-remote-control",
            "playlist-read-private",
            "playlist-read-collaborative",
            "user-read-playback-position",
            "user-library-modify",
            "playlist-modify-private",
            "playlist-modify-public",
            "user-read-email",
            "user-top-read",
            "user-read-recently-played",
            "user-library-read"
        };

        private static HttpListener _httpListner;
        private static Config _config;

        public static async void Authorize(Action<string> tokenCallback = null)
        {
            ReadConfigFile();

            // Generates state and PKCE values.
            string state = RandomDataBase64URL(32);
            string codeVerifier = RandomDataBase64URL(32);
            string codeChallenge = Base64URLEncodeNoPadding(SHA256(codeVerifier));
            const string codeChallengeMethod = "S256";

            // Creates an HttpListener to listen for requests on that redirect URI.
            _httpListner = new HttpListener();
            _httpListner.Prefixes.Add(_redirectUri + "/");
            _httpListner.Start();
            Debug.Log("Listening..");

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?client_id={1}&response_type={2}&redirect_uri={3}&state={4}&scope={5}&code_challenge_method={6}&code_challenge={7}&show_dialog={8}",
                _authorizationEndpoint,
                _config.CLIENT_ID,
                _responseType,
                System.Uri.EscapeDataString(_redirectUri),
                state,
                System.Uri.EscapeDataString(string.Join(" ", _scopes)),
                codeChallengeMethod,
                codeChallenge,
                "false"
                );

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await _httpListner.GetContextAsync();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            var responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                _httpListner.Stop();
                Debug.Log("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Debug.Log(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                Debug.Log("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                Debug.Log(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }

            Debug.Log("Authorization code: " + code);

            var form = new WWWForm();
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", code);
            form.AddField("redirect_uri", _redirectUri);
            form.AddField("client_id", _config.CLIENT_ID);
            form.AddField("code_verifier", codeVerifier);

            var tokenRequest = UnityWebRequest.Post(_tokenEndpoint, form);
            tokenRequest.SetRequestHeader("Authorization", "Basic " + _config.Authorization);
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            await tokenRequest.SendWebRequest();

            Debug.Log(tokenRequest.downloadHandler.text);

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(tokenRequest.downloadHandler.text);

            PlayerPrefs.SetString("SPOTIFY_REFRESH_TOKEN", tokenResponse.refresh_token);
            PlayerPrefs.Save();

            tokenCallback?.Invoke(tokenResponse.access_token);
        }

        //public static void LogRefreshToken()
        //{
        //    Debug.Log(PlayerPrefs.GetString("SPOTIFY_REFRESH_TOKEN"));
        //}

        public static async void RefreshAccessToken(Action<string> tokenCallback = null)
        {
            ReadConfigFile();

            var form = new WWWForm();
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", PlayerPrefs.GetString("SPOTIFY_REFRESH_TOKEN"));
            form.AddField("client_id", _config.CLIENT_ID);

            var tokenRequest = UnityWebRequest.Post(_tokenEndpoint, form);
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            await tokenRequest.SendWebRequest();

            Debug.Log(tokenRequest.downloadHandler.text);

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(tokenRequest.downloadHandler.text);

            PlayerPrefs.SetString("SPOTIFY_REFRESH_TOKEN", tokenResponse.refresh_token);
            PlayerPrefs.Save();

            tokenCallback?.Invoke(tokenResponse.access_token);
        }

        //public static void Abort()
        //{
        //    if (_httpListner != null)
        //    {
        //        _httpListner.Abort();
        //        _httpListner = null;
        //    }
        //}

        private static string RandomDataBase64URL(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64URLEncodeNoPadding(bytes);
        }

        private static byte[] SHA256(string inputString)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputString);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        private static string Base64URLEncodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        private static void ReadConfigFile()
        {
            if (_config == null)
            {
                Debug.Log("Reading Spotify Config File");

                _config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText(Application.dataPath + "/../Config/Spotify.json"));

                if (_config == null || string.IsNullOrWhiteSpace(_config.CLIENT_ID) || string.IsNullOrWhiteSpace(_config.CLIENT_SECRET))
                    throw new Exception("Invalid Spotify Config File");
            }
        }
    }
}

// Help from this article
// https://developer.okta.com/blog/2020/08/21/unity-csharp-games-security