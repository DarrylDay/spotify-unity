using System;
using UnityEngine;
using Spotify.Auth;

namespace Spotify.Android
{
    public sealed class AndroidPlayer : PlayerBackend
    {
        public override Vector3 TrackImageScaleAdjustment => new Vector3(1f, -1f, 1f);

        private AndroidJavaObject _spotifyAppRemote;

        public AndroidPlayer() : base()
        {
        }

        public override ICallResult Init()
        {
            return ConnectToRemote();
        }

        public override ICallResult Pause()
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                return new NativeCallResult(playerAPI.Call<AndroidJavaObject>("pause"));
            }
        }

        public override ICallResult Play(string uri)
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                return new NativeCallResult(playerAPI.Call<AndroidJavaObject>("play", uri));
            }
        }

        public override ICallResult Queue(string uri)
        {
            throw new NotImplementedException();
        }

        public override ICallResult Resume()
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                return new NativeCallResult(playerAPI.Call<AndroidJavaObject>("resume"));
            }
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
                return new NativeCallResult(playerAPI.Call<AndroidJavaObject>("skipNext"));
            }
        }

        public override ICallResult SkipPrevious()
        {
            using (var playerAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getPlayerApi"))
            {
                return new NativeCallResult(playerAPI.Call<AndroidJavaObject>("skipPrevious"));
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            DisconnectFromRemote();
        }

        private ICallResult ConnectToRemote()
        {
            var callResult = new CallResult();

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject unityPlayerActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var builder = new AndroidJavaObject("com.spotify.android.appremote.api.ConnectionParams$Builder", Config.Instance.CLIENT_ID))
            {
                builder.Call<AndroidJavaObject>("setRedirectUri", Config.Instance.ANDROID_AUTH_REDIRECT_URI).Dispose();
                builder.Call<AndroidJavaObject>("showAuthView", false).Dispose();
                using (var connectionParams = builder.Call<AndroidJavaObject>("build"))
                {
                    using (var spotifyAppRemoteClass = new AndroidJavaClass("com.spotify.android.appremote.api.SpotifyAppRemote"))
                    {
                        spotifyAppRemoteClass.CallStatic("connect", unityPlayerActivity, connectionParams, new ConnectionListner(
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

                                callResult.SetFinished();
                            },
                            (error) =>
                            {
                                Debug.LogError("Connection Failed: " + error);

                                callResult.SetError(new Exception(error));
                            }
                        ));
                    }
                }
            }

            return callResult;
        }

        private void DisconnectFromRemote()
        {
            if (_spotifyAppRemote != null)
            {
                using (var spotifyAppRemoteClass = new AndroidJavaClass("com.spotify.android.appremote.api.SpotifyAppRemote"))
                {
                    spotifyAppRemoteClass.CallStatic("disconnect", _spotifyAppRemote);
                }

                _spotifyAppRemote.Dispose();
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
                IsPaused = playerState.Get<bool>("isPaused");

                if (CurrentTrack == null || trackUri != CurrentTrack.Uri)
                {
                    LoadTrackImage(track);
                }

                CurrentTrack = new Track()
                {
                    Name = trackName,
                    Album = new Album()
                    {
                        Name = "",
                        Uri = ""
                    },
                    MainArtist = new Artist()
                    {
                        Name = artistName,
                        Uri = ""
                    },
                    Duration = songLength,
                    Uri = trackUri
                };

                StateUpdated(playbackPosition);
            }
        }

        private void LoadTrackImage(AndroidJavaObject track)
        {
            using (var imageUri = track.Get<AndroidJavaObject>("imageUri"))
            using (var imagesAPI = _spotifyAppRemote.Call<AndroidJavaObject>("getImagesApi"))
            using (var imageBitmapCallResult = imagesAPI.Call<AndroidJavaObject>("getImage", imageUri))
            {
                imageBitmapCallResult.Call<AndroidJavaObject>("setResultCallback", new ResultCallback<AndroidJavaObject>((imageBitmap) =>
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

                    imageBitmap.Dispose();
                }));
            }
        }
    }
}


