namespace Connecty.Application.Models;

public class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required int YearOfRelease { get; set; }
    public string Slug => CreateSlug();
    public required List<string> Genres { get; init; } = [];

    private string CreateSlug()
    {
        string title = Title.Replace(' ', '-');
        string slug = $"{title}-{YearOfRelease}";
        return slug;
    }
}