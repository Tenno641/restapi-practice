namespace Connecty.Contracts.Responses;

public class MoviesResponse
{
    public IEnumerable<MovieResponse> Items { get; init; } = Enumerable.Empty<MovieResponse>();
    public int? Page { get; init; } = 1;
    public int? PageSize { get; init; } = 25;
    public int Total { get; init; }
    public bool Readable { get; init; }
}