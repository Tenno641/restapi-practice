namespace Connecty.Application.Models;

public class GetMoviesOptions : PaginationOptions
{
    public string? Title { get; set; }
    public int? Year { get; set; }
    public Guid? UserId { get; set; }
    public string? SortBy { get; set; }
    public SortOrder SortOrder { get; set; }
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}