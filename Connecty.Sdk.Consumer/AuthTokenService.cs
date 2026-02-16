using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Connecty.Contracts.Requests;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Connecty.Sdk.Consumer;

public class AuthTokenService
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly HttpClient _httpClient;
    private string _cachedToken;
    
    public AuthTokenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedToken))
        {
            JwtSecurityToken? securityToken = new JwtSecurityTokenHandler().ReadJwtToken(_cachedToken);
            string? expirationTimeText = securityToken.Claims.SingleOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value;
            DateTime expirationTime = UnixTimeStampToDateTime(int.Parse(expirationTimeText ?? ""));

            if (expirationTime > DateTime.UtcNow)
            {
                return _cachedToken;
            }
        }

        await _lock.WaitAsync();
        HttpResponseMessage tokenResponse = await _httpClient.PostAsJsonAsync<CreateTokenRequest>("https://localhost:7085/api/Identity", new CreateTokenRequest
        {
            Email = "Custom@Example.com",
            UserId = Guid.NewGuid().ToString(),
            CustomClaims = new Dictionary<string, object>()
            {
                {"admin", true},
                {"trusted_member", true}
            }
        });
        
        string token = await tokenResponse.Content.ReadAsStringAsync();
        
        _cachedToken = token;
        
        _lock.Release();
        
        return token;
    }

    private static DateTime UnixTimeStampToDateTime(int unixStamp)
    {
        DateTime time = DateTimeOffset.FromUnixTimeSeconds(unixStamp).LocalDateTime;
        return time;
    }
}