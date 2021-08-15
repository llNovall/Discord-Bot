using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tiny_Bot.DataClasses;

namespace Tiny_Bot.Services
{
    class GIFTenorService
    {
        

        private string _apikey = "";

        public async Task<string> GetGIFUrl(string searchTerm, int limit)
        {
            string request = $"https://g.tenor.com/v1/random?q={searchTerm}&key={_apikey}&limit={limit}";

            string json = await GetGIFAsync(request);

            if (!string.IsNullOrEmpty(request))
            {
                TenorData gifData = JsonConvert.DeserializeObject<TenorData>(json);
                Random random = new Random();
                return gifData.Results[random.Next(0, limit - 1)].Media[0].Gif.Url;
            }

            return null;
        }

        private async Task<string> GetGIFAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
            if(response.StatusCode == HttpStatusCode.OK)
            {
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            else
            {
                Console.WriteLine($"Failed to find response {response.StatusCode}.");
            }

            return null;
        }
    }
}
