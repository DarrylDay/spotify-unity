﻿using System;
using System.Collections.Generic;

namespace Spotify.WebAPI
{
    public class ItemsBaseResponse<T>
    {
        public List<T> items;
        public string href;
        public int limit;
        public string next;
        public int offset;
        public object previous;
        public int total;
    }

    public class UserPlaylist
    {
        public class Tracks
        {
            public string href { get; set; }
            public int total { get; set; }
        }
        
        public bool collaborative { get; set; }
        public string description { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
        public Owner owner { get; set; }
        public bool @public { get; set; }
        public string snapshot_id { get; set; }
        public Tracks tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class UserSavedTrack
    {
        public DateTime added_at;
        public Track track;
    }

    public class GetPlaybackStateResponse
    {
        public class Item
        {
            public Album album { get; set; }
            public List<Artist> artists { get; set; }
            public List<string> available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            public bool @explicit { get; set; }
            public ExternalIds external_ids { get; set; }
            public ExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public bool is_local { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
        }

        public Device device { get; set; }
        public string repeat_state { get; set; }
        public string shuffle_state { get; set; }
        public Context context { get; set; }
        public long timestamp { get; set; }
        public long progress_ms { get; set; }
        public bool is_playing { get; set; }
        public Item item { get; set; }
        public string currently_playing_type { get; set; }
        public Actions actions { get; set; }
    }
}