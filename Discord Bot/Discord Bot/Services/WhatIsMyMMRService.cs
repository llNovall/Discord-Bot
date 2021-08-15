using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tiny_Bot.DataClasses;

namespace Tiny_Bot.Services
{
    class WhatIsMyMMRService
    {
        private List<string> _allowedRegions;

        public WhatIsMyMMRService()
        {
            _allowedRegions = new List<string>
            {
                "na",
                "euw"
            };
        }

        public async Task<WhatISMyMMRData> FindMMRFor(string region, string userName)
        {
            if (_allowedRegions.Contains(region.ToLower().Trim()))
            {
                string json = await GetDataFromWhatIsMyMMR(region.ToLower().Trim(), userName);

                WhatISMyMMRData jsonObject = JsonConvert.DeserializeObject<WhatISMyMMRData>(json);

                return jsonObject;
            }

            return null;
        }

        private async Task<string> GetDataFromWhatIsMyMMR(string region, string userName)
        {
            string uri = $"https://{region}.whatismymmr.com/api/v1/summoner?name={userName}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("user-agent", "windows:discord-bot:v0.0.1");
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader streamReader = new StreamReader(stream))
            {
                return await streamReader.ReadToEndAsync();
            }
        }
    }
}
