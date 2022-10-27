using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spotify.WebAPI;
using Spotify;

public class TrackCellView : MonoBehaviour
{
    [SerializeField] private RawImage _trackImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _artistsText;
    [SerializeField] private Button _button;

    private Track _track;

    public void Setup(Track track, GameObject tracksPageGO, PlayerFrontend spotifyManager)
    {
        _track = track;

        _nameText.text = track.name;

        string artists = "";
        _track.artists.ForEach(x => artists += x.name + ",");
        artists = artists.TrimEnd(',');
        _artistsText.text = artists;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            Debug.Log(track.uri);
            //spotifyManager.PlaySong(track.uri);
            tracksPageGO.SetActive(false);
        });
    }
}
