using Connecty.Application.Models;

namespace Connecty.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken = default); 
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> AllAsync(GetMoviesOptions options, CancellationToken cancellationToken = default);
    Task<Movie?> GetAsync(Guid id, CancellationToken cancellationToken = default ,Guid? userId = default);
    Task<Movie?> GetAsync(string slug, CancellationToken cancellationToken = default, Guid? userId = default);
    Task<int> GetTotalCountAsync(string? title, int? year, CancellationToken cancellationToken = default);
}