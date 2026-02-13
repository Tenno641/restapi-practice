namespace ExternalIdentity;

public class CreateTokenRequest
{
    public required string Email { get; init; }
    public required string UserId { get; init; }
    public Dictionary<string, object> CustomClaims { get; init; } = [];
}