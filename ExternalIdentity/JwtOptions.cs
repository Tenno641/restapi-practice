namespace ExternalIdentity;

public class JwtOptions
{
    public required string Key { get; init; }
    public required int ExpirationTimeInSeconds { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
}