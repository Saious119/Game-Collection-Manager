using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace GameCollectionManager.Client.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;

    public CustomAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/auth/user");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<DiscordUserDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (userInfo != null)
                {
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, userInfo.Id),
                        new(ClaimTypes.Name, userInfo.Username),
                        new("avatar", userInfo.Avatar ?? "")
                    };

                    var identity = new ClaimsIdentity(claims, "discord");
                    var user = new ClaimsPrincipal(identity);

                    return new AuthenticationState(user);
                }
            }
        }
        catch
        {
            // User not authenticated
        }

        var anonymousIdentity = new ClaimsIdentity();
        return new AuthenticationState(new ClaimsPrincipal(anonymousIdentity));
    }

    public async Task LoginAsync()
    {
        // Redirect to backend OAuth initiation endpoint
        var authUrl = $"{_httpClient.BaseAddress}api/auth/discord/login";
        if (authUrl.EndsWith("/"))
            authUrl = authUrl[..^1];
        
        // Open Discord login in new window, then check auth state
        var window = "name=discord_login,width=600,height=700";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = authUrl,
            UseShellExecute = true
        });

        // Wait a moment then refresh state
        await Task.Delay(2000);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync("api/auth/logout", null);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

public class DiscordUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}