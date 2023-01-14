using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Discord_Bot.Services.DataClasses;

namespace Discord_Bot.Services
{
    internal class GIFTenorService
    {
        private string _apikey = "AIzaSyBSnnoeNrkFxJHHIy1qQ9yrwyTwDVIlS7Q";

        public async Task<string> GetGIFUrl(string searchTerm, int limit)
        {
            string request = $"https://tenor.googleapis.com/v2/search?q={searchTerm}&key={_apikey}&limit={limit}";

            string json = await GetGIFAsync(request);

            if (!string.IsNullOrEmpty(request))
            {
                TenorData gifData = JsonConvert.DeserializeObject<TenorData>(json);
                Random random = new Random();
                return gifData.Results[random.Next(0, limit - 1)].Url;
            }

            return null;
        }

        private async Task<string> GetGIFAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Failed to find response {response.StatusCode}.");
                return null;
            }

            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}