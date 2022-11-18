using System;
using System.Collections;
using UnityEngine;

namespace Spotify
{
    public abstract class PlayerBackend : IDisposable
    {
        public class Track
        {
            public string Name;
            public Album Album;
            public Artist MainArtist;
            //public List<Artist> Artists;
            public long Duration;
            public string Uri;
        }

        public class Artist
        {
            public string Name;
            public string Uri;
        }

        public class Album
        {
            public string Name;
            public string Uri;
        }

        public delegate void TimelineUpdateDelegate(long position, long duration);
        public event TimelineUpdateDelegate OnTimelineTick;
        public event Action<PlayerBackend> OnStateChange;

        // TODO? Change to OnControlsUpdate && OnTrackChange

        public Track CurrentTrack { get; protected set; }
        public Texture2D TrackImage { get; protected set; }
        public virtual Vector3 TrackImageScaleAdjustment => Vector3.one;
        public bool IsPaused { get; protected set; } = true;
        public bool NoPlayer { get; protected set; } = true;
        public long PlaybackPosition => _timelineStopwatch.ElapsedMilliseconds;

        private ExtendedStopwatch _timelineStopwatch = new ExtendedStopwatch();
        private Coroutine _timelineCoroutine;
        private bool _trackFinished;

        public PlayerBackend()
        {
            _timelineCoroutine = MonoBehaviourHelper.RunCoroutine(TimelineUpdateLoop());
            TrackImage = new Texture2D(2, 2);
        }

        public abstract ICallResult Init();

        public abstract ICallResult Play(string uri);
        public abstract ICallResult Queue(string uri);
        public abstract ICallResult Pause();
        public abstract ICallResult Resume();
        public abstract ICallResult SkipNext();
        public abstract ICallResult SkipPrevious();

        public abstract ICallResult SeekTo(long positionMs);
        public abstract ICallResult SeekToRelativePosition(long ms);
        public abstract ICallResult SetRepeat(int repeatMode);
        public abstract ICallResult SetShuffle(bool enabled);

        public virtual void Dispose()
        {
            MonoBehaviourHelper.AbortCoroutine(_timelineCoroutine);
            Texture2D.Destroy(TrackImage);
        }

        protected void StateUpdated(long playbackPosition = -1)
        {
            if (playbackPosition >= 0 && CurrentTrack != null && playbackPosition < CurrentTrack.Duration)
            {
                _timelineStopwatch.Restart(TimeSpan.FromMilliseconds(playbackPosition));
                _trackFinished = false;
            }

            if (IsPaused) _timelineStopwatch.Stop();
            else _timelineStopwatch.Start();

            OnStateChange?.Invoke(this);

            OnTimelineUpdate();
        }

        protected virtual void OnTrackFinish() { }

        private IEnumerator TimelineUpdateLoop()
        {
            while (true)
            {
                while (NoPlayer)
                    yield return null;
                
                if (!IsPaused && CurrentTrack != null)
                {
                    OnTimelineUpdate();
                }

                yield return null;
            }
        }

        private void OnTimelineUpdate()
        {
            if (NoPlayer)
            {
                OnTimelineTick?.Invoke(0, 0);
            }
            else
            {
                if (!_trackFinished && _timelineStopwatch.ElapsedMilliseconds >= CurrentTrack.Duration)
                {
                    OnTrackFinish();
                    _trackFinished = true;
                }

                OnTimelineTick?.Invoke(_timelineStopwatch.ElapsedMilliseconds >= CurrentTrack.Duration ?
                        CurrentTrack.Duration : _timelineStopwatch.ElapsedMilliseconds,
                    CurrentTrack.Duration);
            }
        }

    }
}