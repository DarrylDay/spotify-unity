using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Spotify.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace Spotify.Desktop
{
	public static class DesktopAuth
	{
		public static ICallResult<OAuth.AuthCodeResponse> GetAuthCode()
        {
            return new CallResult<OAuth.AuthCodeResponse>(cts => GetAuthCodeAsync(cts));
        }
		
        private static async Task<OAuth.AuthCodeResponse> GetAuthCodeAsync(CancellationTokenSource cts)
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
                throw new Exception(String.Format("OAuth authorization error: {0}.",
                    context.Request.QueryString.Get("error")));
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                throw new Exception("Malformed authorization response. " + context.Request.QueryString);
            }

            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            if (incoming_state != state)
            {
                throw new Exception(String.Format("Received request with invalid state ({0})", incoming_state));
            }

            return new OAuth.AuthCodeResponse()
            {
                Code = code,
                CodeVerifier = codeVerifier
            };
        }
        
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
	}
}