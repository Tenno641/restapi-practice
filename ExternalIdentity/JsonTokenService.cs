using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExternalIdentity;

public class JsonTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IOptions<JwtOptions> _jwtOptions;
    
    public JsonTokenService(IConfiguration configuration, IOptions<JwtOptions> jwtOptions)
    {
        _configuration = configuration;
        _jwtOptions = jwtOptions;
    }
    
    public string GenerateToken(CreateTokenRequest request)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, request.UserId),
            new Claim(JwtRegisteredClaimNames.Email, request.Email)
        ];

        foreach (var claim in request.CustomClaims)
        {
            JsonElement jsonElement = (JsonElement)claim.Value;
            
            string valueKind = jsonElement.ValueKind switch
            {
                JsonValueKind.True => ClaimValueTypes.Boolean,
                JsonValueKind.False => ClaimValueTypes.Boolean,
                JsonValueKind.Number => ClaimValueTypes.Double,
                _ => ClaimValueTypes.String
            };
            
            claims.Add(new Claim(claim.Key, claim.Value.ToString()!, valueKind));
        }
        
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentException("Please Provide Token Key")));

        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            signingCredentials: signingCredentials,
            issuer: _jwtOptions.Value.Issuer,
            audience: _jwtOptions.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_jwtOptions.Value.ExpirationTimeInSeconds)
        );

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

        string token = tokenHandler.WriteToken(jwtSecurityToken);
        
        return token;
    }
}