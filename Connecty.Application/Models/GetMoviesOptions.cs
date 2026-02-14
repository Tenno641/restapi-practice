namespace Connecty.Application.Models;

public class GetMoviesOptions
{
    public string? Title { get; set; }
    public int? Year { get; set; }
    public Guid? UserId { get; set; } = default;
}