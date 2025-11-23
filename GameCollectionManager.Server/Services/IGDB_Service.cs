using GameCollectionManager.Shared.Models;
using Newtonsoft.Json;
using System.Text;

namespace GameCollectionManagerAPI.Services;

public class IGDB_Service : IIGDB_Service
{
    public string baseIGDBUrl = "https://api.igdb.com/v4/games";
    public string coversUrl = "https://api.igdb.com/v4/covers";
    public string multiplayerUrl = "https://api.igdb.com/v4/multiplayer_modes";
    public string IGDBTokenUrl = String.Format("https://id.twitch.tv/oauth2/token?client_id={0}&client_secret={1}&grant_type=client_credentials", StaticVariables.IGDB_CLIENT_ID, StaticVariables.IGDB_CLIENT_SECRET);
    public async Task<Game> GetIGDBInfo(string gameName)
    {
        var authToken = await GetIGDBToken();
        var client = new HttpClient();
        
        // First try: exact name match using 'where' clause
        var exactMatchContent = new StringContent(
            String.Format("where name ~ *\"{0}\"*; fields id,aggregated_rating,cover,release_dates.human, genres.name,involved_companies.company.name,multiplayer_modes,name,platforms.name,summary; limit 5;", 
            gameName), 
            Encoding.UTF8, 
            "text/plain");
        
        var exactRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(baseIGDBUrl),
            Content = exactMatchContent,
            Headers =
            {
                { "Client-ID", StaticVariables.IGDB_CLIENT_ID },
                { "Authorization", authToken },
            }
        };
        
        using (var response = await client.SendAsync(exactRequest))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            List<Game> results = JsonConvert.DeserializeObject<List<Game>>(body);
            
            if (results != null && results.Any())
            {
                // Try to find an exact match (case-insensitive)
                var exactMatch = results.FirstOrDefault(g => 
                    string.Equals(g.name, gameName, StringComparison.OrdinalIgnoreCase));
                
                if (exactMatch != null)
                {
                    Console.WriteLine($"Found exact match for '{gameName}': {exactMatch.name}");
                    return exactMatch;
                }
                
                // Return the first result if no exact match
                Console.WriteLine($"No exact match for '{gameName}', using first result: {results.First().name}");
                return results.First();
            }
        }
        
        // Fallback to search if exact match fails
        var searchContent = new StringContent(
            String.Format("search \"{0}\"; fields id,aggregated_rating,cover,release_dates.human, genres.name,involved_companies.company.name,multiplayer_modes,name,platforms.name,summary; limit 5;", 
            gameName), 
            Encoding.UTF8, 
            "text/plain");
        
        var searchRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(baseIGDBUrl),
            Content = searchContent,
            Headers =
            {
                { "Client-ID", StaticVariables.IGDB_CLIENT_ID },
                { "Authorization", authToken },
            }
        };
        
        using (var response = await client.SendAsync(searchRequest))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            List<Game> results = JsonConvert.DeserializeObject<List<Game>>(body);
            Console.WriteLine($"Search results for '{gameName}': {body}");
            
            if (results != null && results.Any())
            {
                // Try to find a case-insensitive match
                var match = results.FirstOrDefault(g => 
                    string.Equals(g.name, gameName, StringComparison.OrdinalIgnoreCase));
                
                return match ?? results.First();
            }
            
            throw new Exception($"No results found for '{gameName}'");
        }
    }
    public async Task<List<Game>> SearchIGDBInfo(string gameName)
    {
        var authToken = await GetIGDBToken();
        var client = new HttpClient();
        var content = new StringContent(String.Format("search \"{0}\"; fields id,aggregated_rating,cover,release_dates.human, genres.name,involved_companies.company.name,multiplayer_modes,name,platforms.name,summary; limit 10;", gameName), Encoding.UTF8, "text/plain");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(baseIGDBUrl),
            Content = content,
            Headers =
            {
                { "Client-ID", StaticVariables.IGDB_CLIENT_ID },
                { "Authorization", authToken },
            }
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            List<Game> results = JsonConvert.DeserializeObject<List<Game>>(body);
            Console.WriteLine(body);
            return results;
        }
    }

    public async Task<string> GetCoverArt(int CoverID)
    {
        var authToken = await GetIGDBToken();
        var client = new HttpClient();
        var content = new StringContent(String.Format("where id={0}; fields *;", CoverID), Encoding.UTF8, "text/plain");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(coversUrl),
            Content = content,
            Headers =
            {
                { "Client-ID", StaticVariables.IGDB_CLIENT_ID },
                { "Authorization", authToken },
            }
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            List<GameCover> results = JsonConvert.DeserializeObject<List<GameCover>>(body);
            Console.WriteLine(body);
            return results.First().Url;
        }
    }
    public async Task<MultiplayerModes> GetMultiplayerModes(int gameID)
    {
        var authToken = await GetIGDBToken();
        var client = new HttpClient();
        var content = new StringContent(String.Format("where game={0}; fields *; limit 1;", gameID), Encoding.UTF8, "text/plain");
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(multiplayerUrl),
            Content = content,
            Headers =
            {
                { "Client-ID", StaticVariables.IGDB_CLIENT_ID },
                { "Authorization", authToken },
            }
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            List<MultiplayerModes> results = JsonConvert.DeserializeObject<List<MultiplayerModes>>(body);
            Console.WriteLine(body);
            return results.First();
        }
    }
    public async Task<string> GetIGDBToken()
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(IGDBTokenUrl),
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            IGDBToken results = JsonConvert.DeserializeObject<IGDBToken>(body);
            return "Bearer "+results.access_token;
        }
    }
}