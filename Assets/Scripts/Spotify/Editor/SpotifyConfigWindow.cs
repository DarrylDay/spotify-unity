using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Spotify
{
	public class SpotifyConfigWindow : EditorWindow
	{
		[MenuItem("Tools/Spotify/Config Window")]
		public static void ShowWindow()
        {
			var window = GetWindow(typeof(SpotifyConfigWindow));

            var icon = Resources.Load<Texture2D>("Spotify-Gray");
            window.titleContent = new GUIContent("Spotify Config", icon);
		}

        private const float _padding = 10f;
        private const float _labelWidth = 150f;
        private string[] _scopeOptions;
        private int _scopesMask;

        void OnEnable()
        {
            var scopes = Config.Instance.SCOPES;
            _scopeOptions = Config.ALL_SCOPES.ToArray();
            _scopesMask = 0;

            for (int i = 0; i < _scopeOptions.Length; i++)
            {
                if (scopes.Contains(_scopeOptions[i])) _scopesMask |= (1 << i);
            }
        }

        void OnGUI()
        {
            DrawHeader("App Configuration");
            DrawConfigInput("Client ID", Config.Instance.CLIENT_ID, (newInput) => Config.Instance.CLIENT_ID = newInput);
            DrawConfigInput("Client Secret", Config.Instance.CLIENT_SECRET, (newInput) => Config.Instance.CLIENT_SECRET = newInput, true);
            //DrawPlayerPrefInput("Client Secret", Config.CLIENT_SECRET_PP.Key, true);
            DrawAPIScopes();

            DrawHeader("Desktop Configuration");
            DrawConfigInput("Auth Redirect URI", Config.Instance.DESKTOP_AUTH_REDIRECT_URI, (newInput) => Config.Instance.DESKTOP_AUTH_REDIRECT_URI = newInput);

            DrawHeader("Android Configuration");
            DrawConfigInput("Auth Redirect URI", Config.Instance.ANDROID_AUTH_REDIRECT_URI, (newInput) => Config.Instance.ANDROID_AUTH_REDIRECT_URI = newInput);

            DrawHeader("Cached Data");
            DrawInputField("Saved Refresh Token", Auth.OAuth.SavedRefreshToken);
            DrawButton("Open Cache Folder", () => {
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/SpotifyCache");
                EditorUtility.RevealInFinder(Application.persistentDataPath + "/SpotifyCache");
            });
        }

        private void DrawAPIScopes()
        {
            var newScopes = EditorGUILayout.MaskField("API Scopes", _scopesMask, _scopeOptions);

            if (newScopes != _scopesMask)
            {
                _scopesMask = newScopes;
                var selectedOptions = new List<string>();
                for (int i = 0; i < _scopeOptions.Length; i++)
                {
                    if ((_scopesMask & (1 << i)) == (1 << i)) selectedOptions.Add(_scopeOptions[i]);
                }
                Config.Instance.SCOPES = selectedOptions.ToArray();
                Config.Instance.Save();
            }
        }

        private void DrawConfigInput(string label, string input, Action<string> onChange, bool sensitive = false)
        {
            DrawInputField(label, input, (newInput) =>
            {
                onChange?.Invoke(newInput);
                Config.Instance.Save();
            }, sensitive);
        }

        private void DrawPlayerPrefInput(string label, string playerPrefKey, bool sensitive = false)
        {
            DrawInputField(label, PlayerPrefs.GetString(playerPrefKey), (newInput) =>
            {
                PlayerPrefs.SetString(playerPrefKey, newInput);
                PlayerPrefs.Save();
            }, sensitive);
        }

        private void DrawInputField(string label, string input, Action<string> onChange = null, bool sensitive = false)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, GUILayout.Width(_labelWidth));

            string newInput = "";

            if (sensitive)
            {
                newInput = GUILayout.PasswordField(input, '*');
            }
            else
            {
                newInput = GUILayout.TextField(input);
            }

            if (newInput != input)
            {
                onChange?.Invoke(newInput);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawButton(string text, Action action)
        {
            if (GUILayout.Button(text))
            {
                action?.Invoke();
            }
        }

        private void DrawHeader(string title)
        {
            GUILayout.Space(_padding);

            GUILayout.Label(title, EditorStyles.boldLabel);

            GuiLine();

            GUILayout.Space(5f);
        }

        private void GuiLine(int height = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }
    }
}


