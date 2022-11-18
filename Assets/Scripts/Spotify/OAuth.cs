using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spotify.Android;
using Spotify.Desktop;
using UnityEngine;
using UnityEngine.Networking;

namespace Spotify.Auth
{
    public static class OAuth
    {
        public sealed class TokenHandler
        {
            public string AccessToken => _tokenResponse.access_token;

            private TokenResponse _tokenResponse;
            private List<CallResult<string>> _pendingCallResults = new List<CallResult<string>>();

            public TokenHandler(TokenResponse tokenResponse)
            {
                _tokenResponse = tokenResponse;

                MonoBehaviourHelper.RunCoroutine(RefreshTokenLoop());
            }

            public ICallResult<string> GetAccessTokenSafely()
            {
                var result = new CallResult<string>();

                if (!OAuth.TokenRefreshInProgress)
                {
                    result.SetResult(AccessToken);
                }
                else
                {
                    _pendingCallResults.Add(result);
                }

                return result;
            }

            private IEnumerator RefreshTokenLoop()
            {
                var waitTime = _tokenResponse.expires_in * 0.8f;
                Debug.Log("Refreshing Spotify Auth Token in " + waitTime);

                yield return new WaitForSecondsRealtime(waitTime);

                var result = new CallResult<TokenResponse>((cts) => RefreshAccessToken(cts));
                yield return result.Yield();

                if (result.State == ResultState.Finished)
                {
                    _pendingCallResults.ForEach(x => x.SetResult(AccessToken));
                    _pendingCallResults.Clear();
                    MonoBehaviourHelper.RunCoroutine(RefreshTokenLoop());
                }
            }
        }

        public class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
        }

        public class AuthCodeResponse
        {
            public string Code;
            public string CodeVerifier;
        }
        
        public static readonly PlayerPrefString RefreshTokenPP = new PlayerPrefString("SPOTIFY_REFRESH_TOKEN");
        public static string SavedRefreshToken => RefreshTokenPP.Read();
        public static bool TokenRefreshInProgress { get; private set; }
        public static TokenHandler CurrentTokenHandler { get; private set; }

        public static ICallResult<TokenHandler> Login()
        {
            return new CallResult<TokenHandler>((cts) => LoginAsync(cts));
        }

        public static async Task<TokenHandler> LoginAsync(CancellationTokenSource cts)
        {
            if (CurrentTokenHandler == null)
            {
                if (!string.IsNullOrWhiteSpace(SavedRefreshToken))
                {
                    CurrentTokenHandler = new TokenHandler(await RefreshAccessToken(cts));
                }
                else
                {
                    ICallResult<AuthCodeResponse> callResult = null;
#if UNITY_EDITOR
                    callResult = DesktopAuth.GetAuthCode();
#elif UNITY_ANDROID
                    callResult = AndroidAuth.GetAuthCode();
#else
                    throw new Exception("Platform Not Supported");
#endif

                    var codeResponse = await callResult.AwaitResult();

                    CurrentTokenHandler = new TokenHandler(await GetAccessToken(codeResponse, cts));
                }
            }

            return CurrentTokenHandler;
        }

        private static async Task<TokenResponse> GetAccessToken(AuthCodeResponse codeResponse, CancellationTokenSource cts)
        {
            var form = new WWWForm();
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", codeResponse.Code);
            form.AddField("redirect_uri", Config.Instance.AUTH_REDIRECT_URI);

            if (codeResponse.CodeVerifier != null)
            {
                form.AddField("client_id", Config.Instance.CLIENT_ID);
                form.AddField("code_verifier", codeResponse.CodeVerifier);  
            }

            var tokenRequest = UnityWebRequest.Post(Config.Instance.TOKEN_ENDPOINT, form);
            tokenRequest.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.Instance.CLIENT_ID + ":" + Config.Instance.CLIENT_SECRET)));
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            tokenRequest.RegisterCancellationToken(cts.Token);
            await tokenRequest.SendWebRequest();

            if (!string.IsNullOrWhiteSpace(tokenRequest.error))
                throw new Exception(tokenRequest.error);

            Debug.Log(tokenRequest.downloadHandler.text);

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(tokenRequest.downloadHandler.text);

            RefreshTokenPP.Write(tokenResponse.refresh_token);

            return tokenResponse;
        }

        private static async Task<TokenResponse> RefreshAccessToken(CancellationTokenSource cts)
        {
            if (TokenRefreshInProgress) throw new Exception("Token Refresh Already In Progress");

            TokenRefreshInProgress = true;

            var form = new WWWForm();
            form.AddField("grant_type", "refresh_token");
            form.AddField("refresh_token", SavedRefreshToken);
            form.AddField("client_id", Config.Instance.CLIENT_ID);

            var tokenRequest = UnityWebRequest.Post(Config.Instance.TOKEN_ENDPOINT, form);
            tokenRequest.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.Instance.CLIENT_ID + ":" + Config.Instance.CLIENT_SECRET)));
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            tokenRequest.RegisterCancellationToken(cts.Token);
            await tokenRequest.SendWebRequest();
            
            // Debug.Log(tokenRequest.downloadHandler.text);

            if (!string.IsNullOrWhiteSpace(tokenRequest.error))
            {
                RefreshTokenPP.Delete();
                throw new Exception(tokenRequest.error);
            }

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(tokenRequest.downloadHandler.text);

            if (!string.IsNullOrWhiteSpace(tokenResponse.refresh_token))
                RefreshTokenPP.Write(tokenResponse.refresh_token);

            TokenRefreshInProgress = false;

            Debug.Log("Spotify Auth Token Refreshed");

            return tokenResponse;
        }
    }
}