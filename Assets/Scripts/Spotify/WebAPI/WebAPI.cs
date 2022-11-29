using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Spotify.Auth;
using UnityEngine;
using UnityEngine.Networking;

namespace Spotify.WebAPI
{
    public static class WebAPI
    {
        #region -- Cache Fields --
        
        public static string CacheRootFolder => Application.persistentDataPath + "/SpotifyCache";
        public static string CacheImagesFolder => CacheRootFolder + "/imgs";
        public static FileInfo CachedImage(string id) => new FileInfo(CacheImagesFolder + "/" + id);
        
        #endregion

        #region -- API Requests --

        // TODO: Update to support more than 50
        public static ICallResult<List<UserPlaylist>> GetUserPlaylists()
            => GetItemsResponse<UserPlaylist>(
                UnityWebRequest.kHttpVerbGET,
                "/me/playlists?limit=50");

        public static ICallResult<Texture2D> GetAlbumImage(Album album, Texture2D texture2D = null)
        {
            var result = new CallResult<Texture2D>();
            var texture = texture2D != null ? texture2D : new Texture2D(2, 2);
            
            var fileInfo = CachedImage(album.id);

            if (fileInfo.Exists)
            {
                try
                {
                    texture.LoadImage(File.ReadAllBytes(fileInfo.FullName));
                    result.SetResult(texture);
                    Debug.Log("Cache Loaded");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // Did not have cache 
            if (result.GetState() == ResultState.Pending)
            {
                Debug.Log("No Cache Found");
                WebUtils.DownloadImageBytes(album.images.First().url)
                    .OnResult((bytes) =>
                    {
                        // Save result to cache
                        try
                        {
                            fileInfo.Directory?.Create();
                            File.WriteAllBytes(fileInfo.FullName, bytes);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }

                        texture.LoadImage(bytes);

                        result.SetResult(texture);
                    })
                    .OnError((e) =>
                    {
                        if (texture2D == null) Texture2D.Destroy(texture);
                        result.SetError(e);
                    });
            }

            return result;
        }

        #endregion
        
        #region -- Base Requests --

        // public static ICallResult<T> CheckJsonCache<T>()
        // {
        //     
        // }

        public static ICallResult<List<T>> GetItemsResponse<T>(string method, string path)
        {
            var itemsCall = new CallResult<System.Collections.Generic.List<T>>();
            SendAuthJsonRequest<ItemsBaseResponse<T>>(method, path)
                .OnResult(response =>
                {
                    itemsCall.SetResult(response.items);
                })
                .OnError(itemsCall.SetError);
            return itemsCall;
        }

        public static ICallResult<T> SendAuthJsonRequest<T>(string method, string path, Action onFinish = null) where T : class
        {
            return new CallResult<T>((result) => SendAuthJsonRequestCoroutine(method, path, result, onFinish));
        }
        
        public static ICallResult SendAuthJsonRequest(string method, string path, Action onFinish = null)
        {
            return new CallResult((result) => SendAuthJsonRequestCoroutine(method, path, result, onFinish));
        }

        public static IEnumerator SendAuthJsonRequestCoroutine<T>(string method, string path, CallResult<T> callResult, Action onFinish = null) where T : class
        {
            yield return MonoBehaviourHelper.RunCoroutine(SendAuthJsonRequestCoroutine<T>(method, path,
                (result) => { callResult.SetResult(result); onFinish?.Invoke(); },
                (error) => callResult.SetError(error)));
        }

        public static IEnumerator SendAuthJsonRequestCoroutine<T>(string method, string path, Action<T> onResult = null, Action<Exception> onError = null) where T : class
        {
            if (OAuth.CurrentTokenHandler != null)
            {
                Debug.Log(path);
                
                var tokenResult = OAuth.CurrentTokenHandler.GetAccessTokenSafely();
                yield return MonoBehaviourHelper.RunCoroutine(tokenResult.Yield());

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
        
        #endregion
    }
}