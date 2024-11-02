using GameCollectionManagerAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCollectionManagerAPI.Services
{
    internal class MetaCritic_Services : IMetaCritic_Services
    {
        public async Task<MetacriticData> getMetaCriticInfo(string gameName, string platform)
        {
            gameName = gameName.ToLower();
            gameName = gameName.Replace(" ", "-");
            platform = platform.ToLower();
            platform = platform.Replace(" ", "-");
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://metacriticapi.p.rapidapi.com/games/"+gameName+"?platform="+platform+"&reviews=false"),
                Headers =
                {
                    { "X-RapidAPI-Key", "ebe670b32fmshdf6ddbc82ca645dp107a75jsnc561f8d83610" },
                    { "X-RapidAPI-Host", "metacriticapi.p.rapidapi.com" },
                }
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                MetacriticData results = JsonConvert.DeserializeObject<MetacriticData>(body);
                return results;
            }
        }
    }
}
