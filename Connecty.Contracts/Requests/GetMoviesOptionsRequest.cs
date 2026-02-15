namespace Connecty.Contracts.Requests;

public class GetMoviesOptionsRequest
{
    public string? Title { get; init; }
    public int? Year { get; init; }
    public Guid? UserId { get; init; }
    public required int? Page { get; set; } = 1;
    public required int? PageSize { get; set; } = 15;    
    public string? SortBy { get; init; }
}