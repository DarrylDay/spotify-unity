using System;
using UnityEngine;

namespace Spotify.Android
{
    public class AndroidPlayer : PlayerBackend
    {
        public override Vector3 TrackImageScaleAdjustment => new Vector3(1f, -1f, 1f);

        private const string CLIENT_ID = "a1e7f821360540b6ac02c8c6366229ca";
        private const string REDIRECT_URI = "http://ca.darrylday.spotifytest/callback";

        private AndroidJavaObject _unityPlayerActivity;
        private AndroidJavaObject _spotifyAppRemote;
        private string _accessToken;

        public AndroidPlayer() : base()
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _unityPlayerActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        public override ICallResult Init()
        {
            throw new NotImplementedException();
        }

        public override ICallResult Pause()
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                playerAPI.Call<AndroidJavaObject>("pause").Dispose();
            }

            throw new NotImplementedException();
        }

        public override ICallResult Play(string uri)
        {
            // TODO create CallResult that matches Android CallResult

            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                playerAPI.Call<AndroidJavaObject>("play", uri).Dispose();
            }

            throw new NotImplementedException();
        }

        public override ICallResult Queue(string uri)
        {
            throw new NotImplementedException();
        }

        public override ICallResult Resume()
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                playerAPI.Call<AndroidJavaObject>("resume").Dispose();
            }

            throw new NotImplementedException();
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
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                playerAPI.Call<AndroidJavaObject>("skipNext").Dispose();
            }

            throw new NotImplementedException();
        }

        public override ICallResult SkipPrevious()
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                playerAPI.Call<AndroidJavaObject>("skipPrevious").Dispose();
            }

            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            base.Dispose();
            _unityPlayerActivity?.Dispose();
            DisconnectFromRemote();
        }

        private void GetAuthToken(Action<string> callback)
        {
            using (AndroidJavaClass spotifyLoginActivityClass = new AndroidJavaClass(Application.identifier + ".SpotifyLoginActivity"))
            {
                spotifyLoginActivityClass.CallStatic("SetTokenCallback", new TokenCallback(token =>
                {
                    Debug.Log(token);
                    callback?.Invoke(token);
                }));

                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    using (var spotifyLoginIntent = intent.Call<AndroidJavaObject>("setClassName", _unityPlayerActivity, Application.identifier + ".SpotifyLoginActivity"))
                    {
                        _unityPlayerActivity.Call("startActivity", spotifyLoginIntent);
                    }
                }
            }
        }

        private void ConnectToRemote()
        {
            using (var builder = new AndroidJavaObject("com.spotify.android.appremote.api.ConnectionParams$Builder", CLIENT_ID))
            {
                builder.Call<AndroidJavaObject>("setRedirectUri", REDIRECT_URI).Dispose();
                builder.Call<AndroidJavaObject>("showAuthView", false).Dispose();
                using (var connectionParams = builder.Call<AndroidJavaObject>("build"))
                {
                    using (var spotifyAppRemoteClass = new AndroidJavaClass("com.spotify.android.appremote.api.SpotifyAppRemote"))
                    {
                        spotifyAppRemoteClass.CallStatic("connect", _unityPlayerActivity, connectionParams, new ConnectionListner(
                            (spotifyAppRemote) =>
                            {
                                _spotifyAppRemote = spotifyAppRemote;
                                //_spotifyConnectionOverlay.SetActive(false);
                                Debug.Log("Connection Success!");

                                using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
                                using (var subscription = playerAPI.Call<AndroidJavaObject>("subscribeToPlayerState"))
                                {
                                    subscription.Call<AndroidJavaObject>("setEventCallback", new EventCallback(OnPlayerStateChange)).Dispose();
                                }
                            },
                            (error) =>
                            {
                                Debug.LogError("Connection Failed: " + error);
                            }
                        ));
                    }
                }
            }
        }

        public void DisconnectFromRemote()
        {
            if (_spotifyAppRemote != null)
            {
                Debug.Log("Attempt Disconnect");

                using (var spotifyAppRemoteClass = new AndroidJavaClass("com.spotify.android.appremote.api.SpotifyAppRemote"))
                {
                    spotifyAppRemoteClass.CallStatic("disconnect", _spotifyAppRemote);
                }

                _spotifyAppRemote.Dispose();

                Debug.Log("Disconnected");
            }
        }

        private void OnPlayerStateChange(AndroidJavaObject playerState)
        {
            Debug.Log("Player State Change");

            using (var track = playerState.Get<AndroidJavaObject>("track"))
            using (var artist = track.Get<AndroidJavaObject>("artist"))
            {
                var trackUri = track.Get<string>("uri");
                var trackName = track.Get<string>("name");
                var artistName = artist.Get<string>("name");
                var playbackPosition = playerState.Get<long>("playbackPosition");
                var songLength = track.Get<long>("duration");
                //_isPaused = playerState.Get<bool>("isPaused");

                //UpdatePlayButton();

                //_songNameText.text = trackName;
                //_artistText.text = artistName;

                //_trackLength = songLength / 1000f;
                //var t = TimeSpan.FromMilliseconds(songLength);
                //_songLengthText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

                //_playbackProgress = playbackPosition / 1000f;
                //_playbackStartTime = Time.realtimeSinceStartup;

                //UpdateTimeline(true);

                //if (trackUri != _trackUri)
                //{
                //    LoadTrackImage(track);
                //}

                //_trackUri = trackUri;
            }
        }

        private void LoadTrackImage(AndroidJavaObject track)
        {
            using (var imageUri = track.Get<AndroidJavaObject>("imageUri"))
            using (var imagesAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getImagesApi"))
            using (var imageBitmapCallResult = imagesAPI.Call<AndroidJavaObject>("getImage", imageUri))
            {
                imageBitmapCallResult.Call<AndroidJavaObject>("setResultCallback", new ResultCallback((imageBitmap) =>
                {
                    using (var configClass = new AndroidJavaClass("android.graphics.Bitmap$Config"))
                    using (var ARGB_8888 = configClass.GetStatic<AndroidJavaObject>("ARGB_8888"))
                    using (var config = imageBitmap.Call<AndroidJavaObject>("getConfig"))
                    using (AndroidJavaClass byteBufferClass = new AndroidJavaClass("java.nio.ByteBuffer"))
                    {
                        var width = imageBitmap.Call<int>("getWidth");
                        var height = imageBitmap.Call<int>("getHeight");
                        var size = imageBitmap.Call<int>("getRowBytes") * height;

                        Debug.Log(width);
                        Debug.Log(height);
                        Debug.Log(size);
                        Debug.Log(AndroidJNI.IsSameObject(ARGB_8888.GetRawObject(), config.GetRawObject()));

                        using (var byteBuffer = byteBufferClass.CallStatic<AndroidJavaObject>("allocate", size))
                        {
                            imageBitmap.Call("copyPixelsToBuffer", byteBuffer);
                            using (var nativeByteArray = byteBuffer.Call<AndroidJavaObject>("array"))
                            {
                                var bytes = AndroidJNIHelper.ConvertFromJNIArray<byte[]>(nativeByteArray.GetRawObject());

                                TrackImage.Reinitialize(width, height, TextureFormat.RGBA32, false);
                                TrackImage.LoadRawTextureData(bytes);
                                TrackImage.Apply();
                            }
                        }
                    }
                }));
            }
        }
    }
}


