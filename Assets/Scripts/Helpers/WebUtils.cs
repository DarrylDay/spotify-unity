
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class WebUtils
{
    public static ICallResult<Texture2D> DownloadImageTexture(string url)
    {
        return new CallResult<Texture2D>((result) => DownloadImageTexture(url, result));
    }

    public static IEnumerator DownloadImageTexture(string url, CallResult<Texture2D> result)
    {
        var downloadHandler = new DownloadHandlerTexture();
        var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            result?.SetResult(downloadHandler.texture);
        }
        else
        {
            Debug.LogError("Could not download image");
            Debug.LogError(request.error);
            result?.SetError(request.error);
        }
    }
    
    public static ICallResult<byte[]> DownloadImageBytes(string url)
    {
        return new CallResult<byte[]>((result) => DownloadImageBytes(url, result));
    }
    
    public static IEnumerator DownloadImageBytes(string url, CallResult<byte[]> result)
    {
        Debug.Log(url);
        var downloadHandler = new DownloadHandlerBuffer();
        var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            result?.SetResult(downloadHandler.data);
        }
        else
        {
            Debug.LogError("Could not download image");
            Debug.LogError(request.error);
            result?.SetError(request.error);
        }
    }
}