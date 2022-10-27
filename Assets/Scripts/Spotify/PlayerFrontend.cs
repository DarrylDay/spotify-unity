using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Spotify
{
    public class PlayerFrontend : MonoBehaviour
    {
        [SerializeField] private RawImage _albumImage;
        [SerializeField] private TextMeshProUGUI _songNameText;
        [SerializeField] private TextMeshProUGUI _artistText;
        [SerializeField] private TextMeshProUGUI _currentTimeText;
        [SerializeField] private TextMeshProUGUI _songLengthText;
        [SerializeField] private Slider _timelineSlider;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _playPauseButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Sprite _playSprite;
        [SerializeField] private Sprite _pauseSprite;

        public PlayerBackend Backend { get; private set; }

        private PlayerBackend.Track _currentTrack;

        void OnDestroy()
        {
            if (Backend != null)
            {
                Backend.OnStateChange -= OnPlayerStateChanged;
                Backend.OnTimelineTick -= OnTimelineTick;
            }
        }

        public void Init(PlayerBackend backend)
        {
            if (Backend != null) throw new Exception("Spotify Player Frontend Already Initialized");

            Backend = backend;
            Backend.OnStateChange += OnPlayerStateChanged;
            Backend.OnTimelineTick += OnTimelineTick;

            if (Backend.CurrentTrack != null)
            {
                OnPlayerStateChanged(Backend);
                OnTimelineTick(Backend.PlaybackPosition, Backend.CurrentTrack.Duration);
            }

            _albumImage.texture = Backend.TrackImage;
            _albumImage.transform.localScale = Backend.TrackImageScaleAdjustment;
        }

        public void PlayOrPause()
        {
            if (Backend.IsPaused)
            {
                Backend.Resume();
            }
            else
            {
                Backend.Pause();
            }

            UpdatePlayPauseButton();
        }

        public void SkipPrevious()
        {
            Backend.SkipPrevious();
        }

        public void SkipNext()
        {
            Backend.SkipNext();
        }

        private void OnPlayerStateChanged(PlayerBackend backend)
        {
            if (_currentTrack == null || _currentTrack.Uri != backend.CurrentTrack.Uri)
            {
                _currentTrack = backend.CurrentTrack;

                _songNameText.text = _currentTrack.Name;
                _artistText.text = _currentTrack.MainArtist.Name;

                var t = TimeSpan.FromMilliseconds(_currentTrack.Duration);
                _songLengthText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            }

            UpdatePlayPauseButton();
        }

        private void OnTimelineTick(long pos, long dur)
        {
            var t = TimeSpan.FromMilliseconds(pos);
            _currentTimeText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            _timelineSlider.value = (float)pos / dur;
        }

        private void UpdatePlayPauseButton()
        {
            _playPauseButton.image.sprite = Backend.IsPaused ? _playSprite : _pauseSprite;
        }
    }
}

