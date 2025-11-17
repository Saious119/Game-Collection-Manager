using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameCollectionManagerAPI.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;

namespace GameCollectionManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IConfiguration configuration, IJwtService jwtService, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _jwtService = jwtService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("discord/login")]
        public IActionResult DiscordLogin()
        {
            var clientId = _configuration["Discord:ClientId"];
            var redirectUri = _configuration["Discord:RedirectUri"];
            var scope = "identify email";

            var discordAuthUrl = $"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri!)}&response_type=code&scope={Uri.EscapeDataString(scope)}";

            return Ok(new { authUrl = discordAuthUrl });
        }

        [HttpGet("discord/callback")]
        public async Task<IActionResult> DiscordCallback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code is missing");

            try
            {
                // Exchange code for access token
                var tokenResponse = await ExchangeCodeForToken(code);
                
                // Get user info from Discord
                var userInfo = await GetDiscordUserInfo(tokenResponse.AccessToken);

                // Generate JWT token
                var jwtToken = _jwtService.GenerateToken(
                    userInfo.Id,
                    userInfo.Username,
                    userInfo.Email ?? ""
                );

                // Redirect back to Blazor app with token
                var clientUrl = "https://localhost:7176/authentication/login-callback";
                return Redirect($"{clientUrl}?token={jwtToken}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Authentication failed: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var username = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            return Ok(new { userId, username, email });
        }

        private async Task<DiscordTokenResponse> ExchangeCodeForToken(string code)
        {
            var client = _httpClientFactory.CreateClient();
            var clientId = _configuration["Discord:ClientId"];
            var clientSecret = _configuration["Discord:ClientSecret"];
            var redirectUri = _configuration["Discord:RedirectUri"];

            var parameters = new Dictionary<string, string>
            {
                { "client_id", clientId! },
                { "client_secret", clientSecret! },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri! }
            };

            var response = await client.PostAsync(
                "https://discord.com/api/oauth2/token",
                new FormUrlEncodedContent(parameters)
            );

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            
            var options = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            
            return JsonSerializer.Deserialize<DiscordTokenResponse>(content, options)!;
        }

        private async Task<DiscordUser> GetDiscordUserInfo(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            
            // Create a new request message with proper authorization header
            var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DiscordUser>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        private class DiscordTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; } = "";
            
            [JsonPropertyName("token_type")]
            public string TokenType { get; set; } = "";
            
            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
            
            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; } = "";
            
            [JsonPropertyName("scope")]
            public string Scope { get; set; } = "";
        }

        private class DiscordUser
        {
            public string Id { get; set; } = "";
            public string Username { get; set; } = "";
            public string? Email { get; set; }
            public string? Avatar { get; set; }
        }
    }
}