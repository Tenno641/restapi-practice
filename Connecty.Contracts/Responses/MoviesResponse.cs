namespace Connecty.Contracts.Responses;

public class MoviesResponse
{
    public IEnumerable<MovieResponse> Movies { get; init; } = Enumerable.Empty<MovieResponse>();
}