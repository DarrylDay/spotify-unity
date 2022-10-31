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
                    CurrentTokenHandler = new TokenHandler(await Authorize(cts));
                }
            }

            return CurrentTokenHandler;
        }

        private static async Task<TokenResponse> Authorize(CancellationTokenSource cts)
        {
            // Generates state and PKCE values.
            string state = RandomDataBase64URL(32);
            string codeVerifier = RandomDataBase64URL(32);
            string codeChallenge = Base64URLEncodeNoPadding(SHA256(codeVerifier));
            string codeChallengeMethod = "S256";

            // Creates an HttpListener to listen for requests on that redirect URI.
            var httpListner = new HttpListener();
            httpListner.Prefixes.Add(Config.Instance.DESKTOP_AUTH_REDIRECT_URI + "/");
            httpListner.Start();
            Debug.Log("Listening..");

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?client_id={1}&response_type={2}&redirect_uri={3}&state={4}&scope={5}&code_challenge_method={6}&code_challenge={7}&show_dialog={8}",
                Config.Instance.AUTH_ENDPOINT,
                Config.Instance.CLIENT_ID,
                "code",
                System.Uri.EscapeDataString(Config.Instance.DESKTOP_AUTH_REDIRECT_URI),
                state,
                System.Uri.EscapeDataString(string.Join(" ", Config.Instance.SCOPES)),
                codeChallengeMethod,
                codeChallenge,
                "false"
                );

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await httpListner.GetContextAsync().AsCancellable(cts.Token);

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            var responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                httpListner.Stop();
                Debug.Log("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Debug.Log(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return null;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                Debug.Log("Malformed authorization response. " + context.Request.QueryString);
                return null;
            }

            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            if (incoming_state != state)
            {
                Debug.Log(String.Format("Received request with invalid state ({0})", incoming_state));
                return null;
            }

            //Debug.Log("Authorization code: " + code);

            var form = new WWWForm();
            form.AddField("grant_type", "authorization_code");
            form.AddField("code", code);
            form.AddField("redirect_uri", Config.Instance.DESKTOP_AUTH_REDIRECT_URI);
            form.AddField("client_id", Config.Instance.CLIENT_ID);
            form.AddField("code_verifier", codeVerifier);

            var tokenRequest = UnityWebRequest.Post(Config.Instance.TOKEN_ENDPOINT, form);
            tokenRequest.SetRequestHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.Instance.CLIENT_ID + ":" + Config.Instance.CLIENT_SECRET)));
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            tokenRequest.RegisterCancellationToken(cts.Token);
            await tokenRequest.SendWebRequest();

            //Debug.Log(tokenRequest.downloadHandler.text);

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
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            tokenRequest.RegisterCancellationToken(cts.Token);
            await tokenRequest.SendWebRequest();

            //Debug.Log(tokenRequest.downloadHandler.text);

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(tokenRequest.downloadHandler.text);

            RefreshTokenPP.Write(tokenResponse.refresh_token);

            TokenRefreshInProgress = false;

            Debug.Log("Spotify Auth Token Refreshed");

            return tokenResponse;
        }

        #region -- Helper Methods --

        // https://developer.okta.com/blog/2020/08/21/unity-csharp-games-security

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

        #endregion
    }
}