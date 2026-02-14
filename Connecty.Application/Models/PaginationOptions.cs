namespace Connecty.Application.Models;

public class PaginationOptions
{
    public required int? Page { get; set; } = 1;
    public required int? PageSize { get; set; } = 25;
}