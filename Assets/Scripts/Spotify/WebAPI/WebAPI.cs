using System;
using System.Collections;
using System.IO;
using System.Linq;
using Spotify.Auth;
using UnityEngine;

namespace Spotify.WebAPI
{
    public static class WebAPI
    {
        public static string CacheRootFolder => Application.persistentDataPath + "/SpotifyCache";
        public static string CacheImagesFolder => CacheRootFolder + "/imgs";
        public static FileInfo CachedImage(string id) => new FileInfo(CacheImagesFolder + "/" + id);
        
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
    }
}