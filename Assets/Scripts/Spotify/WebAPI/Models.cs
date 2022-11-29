using System;
using System.Collections.Generic;

namespace Spotify.WebAPI
{
    public class Track
    {
        public Album album;
        public List<Artist> artists;
        public List<string> available_markets;
        public int disc_number;
        public int duration_ms;
        public bool @explicit;
        public ExternalIds external_ids;
        public ExternalUrls external_urls;
        public string href;
        public string id;
        public bool is_local;
        public string name;
        public int popularity;
        public string preview_url;
        public int track_number;
        public string type;
        public string uri;
    }

    public class Artist
    {
        public ExternalUrls external_urls;
        public string href;
        public string id;
        public string name;
        public string type;
        public string uri;
    }

    public class Album
    {
        public string album_type;
        public List<Artist> artists;
        public List<string> available_markets;
        public ExternalUrls external_urls;
        public string href;
        public string id;
        public List<Image> images;
        public string name;
        public string release_date;
        public string release_date_precision;
        public int total_tracks;
        public string type;
        public string uri;
    }

    public class Image
    {
        public int? height;
        public string url;
        public int? width;
    }

    public class ExternalIds
    {
        public string isrc;
    }

    public class ExternalUrls
    {
        public string spotify;
    }

    public class Actions
    {
        public bool interrupting_playback { get; set; }
        public bool pausing { get; set; }
        public bool resuming { get; set; }
        public bool seeking { get; set; }
        public bool skipping_next { get; set; }
        public bool skipping_prev { get; set; }
        public bool toggling_repeat_context { get; set; }
        public bool toggling_shuffle { get; set; }
        public bool toggling_repeat_track { get; set; }
        public bool transferring_playback { get; set; }
    }

    public class Device
    {
        public string id { get; set; }
        public bool is_active { get; set; }
        public bool is_private_session { get; set; }
        public bool is_restricted { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int volume_percent { get; set; }
    }

    public class Context
    {
        public string type { get; set; }
        public string href { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string uri { get; set; }
    }
    
    public class Owner
    {
        public ExternalUrls external_urls { get; set; }
        public Followers followers { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public string display_name { get; set; }
    }
    
    public class Followers
    {
        public string href { get; set; }
        public int total { get; set; }
    }
}