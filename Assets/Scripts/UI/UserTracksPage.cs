using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Spotify.WebAPI;

public class UserTracksPage : MonoBehaviour
{
    //[SerializeField] private PlayerFrontend _spotifyManager;
    [SerializeField] private RectTransform _listRT;
    [SerializeField] private TrackCellView _trackCellViewPrefab;

    private string _accessToken;
    private GetUserSavedTracksResponse _response;

    public void Load(string accessToken)
    {
        if (_response != null) return;

        _accessToken = accessToken;
        StartCoroutine(SendRequest("https://api.spotify.com/v1/me/tracks"));
    }

    private IEnumerator SendRequest(string url)
    {
        if (_accessToken != null)
        {
            var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", "Bearer " + _accessToken);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(request.downloadHandler.text);

                _response = JsonUtility.FromJson<GetUserSavedTracksResponse>(request.downloadHandler.text);

                CreateList();
            }
            else
            {
                Debug.LogError(request.error);
            }
        }
    }

    private void CreateList()
    {
        if (_response != null)
        {
            foreach (Transform t in _listRT)
                GameObject.Destroy(t.gameObject);

            if (_response.items != null)
            {
                _response.items.ForEach(x =>
                {
                    var trackCellView = Instantiate(_trackCellViewPrefab, _listRT);
                    //trackCellView.Setup(x.track, gameObject, _spotifyManager);
                });
            }
        }
    }
}
