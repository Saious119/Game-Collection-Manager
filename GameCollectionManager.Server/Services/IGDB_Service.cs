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
    // IGDB category values: 0=main_game, 1=dlc, 2=expansion, 3=bundle, 4=standalone_expansion,
    // 5=mod, 6=episode, 7=season, 8=remake, 9=remaster, 10=expanded_game, 11=port
    private static readonly HashSet<int> DlcCategories = new HashSet<int> { 1, 2, 5, 6, 7, 12, 13, 14 };

    private const string GameFields = "id,aggregated_rating,category,cover,release_dates.human,genres.name,involved_companies.company.name,multiplayer_modes,name,platforms.name,summary";

    private Game BestMatch(List<Game> results, string gameName)
    {
        var exactMatch = results.FirstOrDefault(g =>
            string.Equals(g.name, gameName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
            return exactMatch;

        // Deprioritize DLC/expansion categories, then prefer names closest in length
        // to the search term (DLC names tend to be longer since they include the base game name).
        return results
            .OrderBy(g => DlcCategories.Contains(g.category) ? 1 : 0)
            .ThenBy(g => Math.Abs(g.name.Length - gameName.Length))
            .ThenBy(g => g.name.StartsWith(gameName, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .First();
    }

    // Strip characters that can break IGDB's Apicalypse query syntax (e.g. colons,
    // quotes, backslashes). Used when building search queries.
    private static string SanitizeSearchTerm(string name) =>
        System.Text.RegularExpressions.Regex.Replace(name, @"[:""\\\[\]{}()]", " ").Trim();

    public async Task<Game> GetIGDBInfo(string gameName)
    {
        var authToken = await GetIGDBToken();
        var client = new HttpClient();

        // Search with the original name first, fall back to a sanitized version if empty.
        var searchTerm = gameName;
        for (int attempt = 0; attempt < 2; attempt++)
        {
            var content = new StringContent(
                String.Format("search \"{0}\"; fields {1}; limit 20;", searchTerm, GameFields),
                Encoding.UTF8,
                "text/plain");

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
                Console.WriteLine($"IGDB search (attempt {attempt + 1}) for '{searchTerm}': {body}");

                if (results != null && results.Any())
                {
                    var best = BestMatch(results, gameName);
                    Console.WriteLine($"Selected: {best.name} (category {best.category})");
                    return best;
                }
            }

            // First attempt returned nothing — retry with sanitized name
            searchTerm = SanitizeSearchTerm(gameName);
            if (searchTerm == gameName) break; // nothing to sanitize, don't retry
        }

        throw new Exception($"No results found for '{gameName}'");
    }
    public async Task<List<Game>> SearchIGDBInfo(string gameName)
    {
        var authToken = await GetIGDBToken();
        var client = new HttpClient();
        var searchTerm = SanitizeSearchTerm(gameName);
        var content = new StringContent(
            String.Format("search \"{0}\"; fields {1}; limit 10;", searchTerm, GameFields),
            Encoding.UTF8, "text/plain");
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
            // Sort so main games appear before DLC/expansions, then by name length
            // so the base game title appears at the top of the identify list.
            return results?
                .OrderBy(g => DlcCategories.Contains(g.category) ? 1 : 0)
                .ThenBy(g => g.name.Length)
                .ToList();
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