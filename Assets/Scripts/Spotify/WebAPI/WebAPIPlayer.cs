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
        public WebAPIPlayer() : base()
        {
        }
        
        public override ICallResult Init()
        {
            var initCall = new CallResult();

            WebAPI.SendAuthJsonRequest<GetPlaybackStateResponse>(
                UnityWebRequest.kHttpVerbGET,
                "/me/player")
                .OnResult(state =>
                {
                    if (state == null)
                    {
                        NoPlayer = true;
                        IsPaused = true;
                    }
                    else
                    {
                        NoPlayer = false;
                        IsPaused = !state.is_playing;

                        var track = state.item;
                        var album = track.album;
                        var artist = track.artists.First();
                        
                        WebAPI.GetAlbumImage(album, TrackImage);

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

                        StateUpdated(state.progress_ms);
                    }
                    
                    initCall.SetResult(CallResult.Empty);
                })
                .OnError(initCall.SetError);

            return initCall;
        }

        public override ICallResult Pause() => WebAPI.SendAuthJsonRequest(
            UnityWebRequest.kHttpVerbPUT,
            "/me/player/pause",
            () => {
                IsPaused = true;
                StateUpdated();
                Init();
            });

        public override ICallResult Play(string uri)
        {
            throw new NotImplementedException();
        }

        public override ICallResult Queue(string uri)
        {
            throw new NotImplementedException();
        }

        public override ICallResult Resume() => WebAPI.SendAuthJsonRequest(
            UnityWebRequest.kHttpVerbPUT,
            "/me/player/play",
            () => {
                IsPaused = false;
                StateUpdated();
                Init();
            });

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

        public override ICallResult SkipNext() => WebAPI.SendAuthJsonRequest(
            UnityWebRequest.kHttpVerbPOST,
            "/me/player/next",
            Refresh);

        public override ICallResult SkipPrevious() => WebAPI.SendAuthJsonRequest(
            UnityWebRequest.kHttpVerbPOST,
            "/me/player/previous",
            Refresh);

        protected override void OnTrackFinish()
        {
            Init();
        }

        private void Refresh()
        {
            Init();
        }
    }
}