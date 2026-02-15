namespace Connecty.Contracts.Responses;

public class MoviesResponse
{
    public IEnumerable<MovieResponse> Items { get; init; } = Enumerable.Empty<MovieResponse>();
    public int? Page { get; init; }
    public int? PageSize { get; init; } 
    public int Total { get; init; }
    public bool HasNextPage => Total > Page * PageSize;
}