using Newtonsoft.Json;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    internal class SpotifyService
    {
        private SpotifyClientConfig _defaultConfig = SpotifyClientConfig.CreateDefault();
        private string _id;
        private string _secret;
        private string _apiKey;

        public SpotifyService()
        {
            string json = "";
            using (FileStream fileStream = File.OpenRead("service.json"))
            using (StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(false)))
                json = streamReader.ReadToEnd();

            var keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            _id = keys["spotify_id"];
            _secret = keys["spotify_secret"];
        }

        public async Task<string> GetAccessToken()
        {
            string url5 = "https://accounts.spotify.com/api/token";
            var clientid = _id;
            var clientsecret = _secret;

            string json = "";
            var encode_clientid_clientsecret = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", clientid, clientsecret)));

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url5);

            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/json";
            webRequest.Headers.Add("Authorization: Basic " + encode_clientid_clientsecret);

            var request = ("grant_type=client_credentials");
            byte[] req_bytes = Encoding.ASCII.GetBytes(request);
            webRequest.ContentLength = req_bytes.Length;

            Stream strm = await webRequest.GetRequestStreamAsync();
            strm.Write(req_bytes, 0, req_bytes.Length);
            strm.Close();

            using (HttpWebResponse resp = (HttpWebResponse)webRequest.GetResponse())
            {
                using (Stream respStr = resp.GetResponseStream())
                {
                    using (StreamReader rdr = new StreamReader(respStr, Encoding.UTF8))
                    {
                        json = rdr.ReadToEnd();
                        rdr.Close();
                    }
                }
            }

            if (string.IsNullOrEmpty(json))
                return "";

            string token = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)["access_token"];
            return token;
        }

        public async Task<List<string>> GetTrackNames(Uri playlistUri)
        {
            string pattern = @"open\.spotify\.com/playlist/(?<id>.*)";
            Regex rg = new Regex(pattern);
            Match match = rg.Match(playlistUri.ToString());
            string playlistID = match.Groups["id"].Value;

            if (string.IsNullOrEmpty(playlistID))
                return new List<string>();

            _apiKey = await GetAccessToken();

            SpotifyClientConfig config = _defaultConfig.WithToken(_apiKey);
            SpotifyClient spotify = new SpotifyClient(config);

            var playlist = await spotify.Playlists.Get(playlistID);
            List<string> trackNames = new();

            foreach (PlaylistTrack<IPlayableItem> item in playlist.Tracks.Items)
            {
                if (item.Track is FullTrack track)
                {
                    trackNames.Add(track.Name + track.Album.Name);
                }
            }

            return trackNames;
        }
    }
}