namespace Connecty.Contracts.Requests;

public class GetMoviesOptionsRequest
{
    public string? Title { get; init; }
    public int? Year { get; init; }
    public Guid? UserId { get; init; }
    public string? SortBy { get; init; }
}