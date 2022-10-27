using System;
using System.Text;
using Newtonsoft.Json;

namespace Spotify.Auth
{
    public class Config
    {
        public string CLIENT_ID;
        public string CLIENT_SECRET;

        [JsonIgnore] public string Authorization => Convert.ToBase64String(Encoding.UTF8.GetBytes(CLIENT_ID + ":" + CLIENT_SECRET));
    }
}