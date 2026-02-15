using System.Text.Json.Serialization;

namespace Connecty.Contracts.Responses;

public class HalResponse // Hypermedia Api Language
{
    public List<Link> Links { get; init; } = [];
}

public class Link
{
    public string? Type { get; set; }
    public string? href { get; init; }
    public string? Rel { get; set; }
}