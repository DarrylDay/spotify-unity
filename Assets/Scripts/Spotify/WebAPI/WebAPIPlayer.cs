using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Spotify.Auth;

namespace Spotify.WebAPI
{
	public sealed class WebAPIPlayer : PlayerBackend
	{
        private OAuth.TokenHandler _tokenHandler;

        public WebAPIPlayer(OAuth.TokenHandler tokenHandler) : base()
        {
            _tokenHandler = tokenHandler;
        }

        public override ICallResult Init()
        {
            var initCall = new CallResult();

            MonoBehaviourHelper.RunCoroutine(SendRequest<GetPlaybackStateResponse>(
                UnityWebRequest.kHttpVerbGET,
                "/me/player",
                (state) =>
                {
                    IsPaused = !state.is_playing;

                    var track = state.item;
                    var album = track.album;
                    var artist = track.artists.First();

                    // TODO add cache manager
                    MonoBehaviourHelper.RunCoroutine(GetAlbumImage(album.images.First().url, (texture) =>
                    {
                        TrackImage.Reinitialize(texture.width, texture.height, texture.format, false);
                        TrackImage.SetPixels32(texture.GetPixels32());
                        TrackImage.Apply();
                        //Texture2D.Destroy(texture);
                        // TODO figure out why this crashes
                    }));

                    CurrentTrack = new Track()
                    {
                        Name = track.name,
                        Album = new Album()
                        {
                            Name = album.name,
                            Uri = album.uri
                        },
                        MainArtist = new Artist()
                        {
                            Name = artist.name,
                            Uri = artist.uri
                        },
                        Duration = track.duration_ms,
                        Uri = track.uri
                    };

                    long progress = state.progress_ms;

                    if (!IsPaused)
                    {
                        var delta = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - state.timestamp);
                        progress += delta;
                    }

                    StateUpdated(progress);

                    initCall.SetResult(CallResult.Empty);
                },
                (error) =>
                {
                    initCall.SetError(error);
                }
                ));

            return initCall;
        }

        public override ICallResult Pause()
        {
            return new CallResult(cr => SendRequest(
                UnityWebRequest.kHttpVerbPUT,
                "/me/player/pause",
                cr,
                () => {
                    IsPaused = true;
                    StateUpdated();
                    Init();
                }));
        }

        public override ICallResult Play(string uri)
        {
            throw new NotImplementedException();
        }

        public override ICallResult Queue(string uri)
        {
            throw new NotImplementedException();
        }

        public override ICallResult Resume()
        {
            return new CallResult(cr => SendRequest(
                UnityWebRequest.kHttpVerbPUT,
                "/me/player/play",
                cr,
                () => {
                    IsPaused = false;
                    StateUpdated();
                    Init();
                }));
        }

        public override ICallResult SeekTo(long positionMs)
        {
            throw new NotImplementedException();
        }

        public override ICallResult SeekToRelativePosition(long ms)
        {
            throw new NotImplementedException();
        }

        public override ICallResult SetRepeat(int repeatMode)
        {
            throw new NotImplementedException();
        }

        public override ICallResult SetShuffle(bool enabled)
        {
            throw new NotImplementedException();
        }

        public override ICallResult SkipNext()
        {
            return new CallResult(cr => SendRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/me/player/next",
                cr,
                () => {
                    Init();
                }));
        }

        public override ICallResult SkipPrevious()
        {
            return new CallResult(cr => SendRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/me/player/previous",
                cr,
                () => {
                    Init();
                }));
        }

        protected override void OnTrackFinish()
        {
            Init();
        }

        private IEnumerator SendRequest<T>(string method, string path, CallResult<T> callResult, Action onFinish = null) where T : class
        {
            yield return MonoBehaviourHelper.RunCoroutine(SendRequest<T>(method, path,
                (result) => { callResult.SetResult(result); onFinish?.Invoke(); },
                (error) => callResult.SetError(error)));
        }

        private IEnumerator SendRequest<T>(string method, string path, Action<T> onResult = null, Action<Exception> onError = null) where T : class
        {
            if (_tokenHandler != null)
            {
                var tokenResult = _tokenHandler.GetAccessTokenSafely();
                yield return MonoBehaviourHelper.RunCoroutine(tokenResult.Yield());

                //while (OAuth.TokenRefreshInProgress) yield return null;

                var request = new UnityWebRequest(Config.Instance.API_ENDPOINT + path, method, new DownloadHandlerBuffer(), null);
                request.SetRequestHeader("Authorization", "Bearer " + tokenResult.GetResult());
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        //Debug.Log(request.downloadHandler.text);

                        if (typeof(Empty) is T)
                        {
                            onResult?.Invoke(CallResult.Empty as T);
                        }
                        else
                        {
                            var response = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                            onResult?.Invoke(response);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);

                        onError?.Invoke(e);
                    }
                }
                else
                {
                    Debug.LogError(request.url);
                    Debug.LogError(request.error);

                    onError?.Invoke(new Exception(request.error));
                }
            }
            else
            {
                onError?.Invoke(new Exception("No Access Token"));
            }
        }

        private IEnumerator GetAlbumImage(string url, Action<Texture2D> onFinish)
        {
            var downloadHandler = new DownloadHandlerTexture();
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onFinish?.Invoke(downloadHandler.texture);
            }
            else
            {
                Debug.LogError("Could not download image");
                Debug.LogError(request.error);
            }
        }
    }
}