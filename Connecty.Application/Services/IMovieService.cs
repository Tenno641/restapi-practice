using Connecty.Application.Models;

namespace Connecty.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> UpdateAsync(Movie movie);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Movie>> AllAsync();
    Task<Movie?> GetAsync(Guid id);
    Task<Movie?> GetAsync(string slug);
}