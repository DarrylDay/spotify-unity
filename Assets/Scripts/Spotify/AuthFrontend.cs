using System;
using Spotify.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spotify
{
    public class AuthFrontend : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private Button _authenticateButton;

        private Action<OAuth.TokenHandler> _onAuth;

        void Awake()
        {
            _canvas.enabled = false;
        }

        public void Setup(Action<OAuth.TokenHandler> onAuth)
        {
            _onAuth = onAuth;
            _canvas.enabled = true;
            _loadingText.gameObject.SetActive(false);
            _authenticateButton.gameObject.SetActive(true);
        }

        public void StartAuthentication()
        {
            _loadingText.gameObject.SetActive(true);
            _authenticateButton.gameObject.SetActive(false);

#if UNITY_EDITOR || UNITY_STANDALONE
            _loadingText.text = "Authenticating in external web browser...";
#else
            _loadingText.text = "Authenticating with Spotify app...";
#endif

            OAuth.Login()
                .OnResult((tokenHandler) =>
                {
                    _canvas.enabled = false;
                    _onAuth?.Invoke(tokenHandler);
                })
                .OnError((e) =>
                {
                    // TODO: Add error overlay
                    Debug.LogException(e);
                    Setup(_onAuth);
                });
        }
    }
}