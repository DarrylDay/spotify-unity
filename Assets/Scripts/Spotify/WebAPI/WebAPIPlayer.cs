using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Spotify.WebAPI
{
	public class WebAPIPlayer : PlayerBackend
	{
        private const string _rootEndpoint = "https://api.spotify.com/v1";
        private string _accessToken;

		public WebAPIPlayer(string accessToken) : base()
		{
            _accessToken = accessToken;
		}

        public override ICallResult Init()
        {
            // TODO fix this
            var initCall = new CallResult();
            var callResult = new CallResult<GetPlaybackStateResponse>(cr => SendRequest(
                UnityWebRequest.kHttpVerbGET,
                "/me/player",
                cr,
                (state) => {

                    IsPaused = !state.is_playing;

                    var track = state.item;
                    var album = track.album;
                    var artist = track.artists.First();

                    // TODO add cache manager
                    CoroutineHelper.Run(GetAlbumImage(album.images.First().url, (texture) =>
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
                }));
            return initCall;
        }

        public override ICallResult Pause()
        {
            return new CallResult(cr => SendRequest(
                UnityWebRequest.kHttpVerbPUT,
                "/me/player/pause",
                cr,
                (e) => {
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
                (e) => {
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
                (e) => {
                    Init();
                }));
        }

        public override ICallResult SkipPrevious()
        {
            return new CallResult(cr => SendRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/me/player/previous",
                cr,
                (e) => {
                    Init();
                }));
        }

        protected override void OnTrackFinish()
        {
            Init();
        }

        private IEnumerator SendRequest<T>(string method, string path, CallResult<T> callResult = null, Action<T> onFinish = null) where T : class
        {
            if (_accessToken != null)
            {
                var request = new UnityWebRequest(_rootEndpoint + path, method, new DownloadHandlerBuffer(), null);
                request.SetRequestHeader("Authorization", "Bearer " + _accessToken);
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        //Debug.Log(request.downloadHandler.text);

                        if (typeof(Empty) is T)
                        {
                            onFinish?.Invoke(CallResult.Empty as T);
                            callResult?.SetResult(CallResult.Empty as T);
                        }
                        else
                        {
                            var response = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
                            onFinish?.Invoke(response);
                            callResult?.SetResult(response);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);

                        callResult?.SetError(e);
                    }
                }
                else
                {
                    Debug.LogError(request.url);
                    Debug.LogError(request.error);

                    callResult?.SetError(new Exception(request.error));
                }
            }
            else
            {
                callResult?.SetError(new Exception("No Access Token"));
            }
        }

        private IEnumerator GetAlbumImage(string url, Action<Texture2D> onFinish)
        {
            //Debug.Log(url);
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