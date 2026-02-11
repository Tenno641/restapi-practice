using Connecty.Application.Models;

namespace Connecty.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie);
    Task<bool> UpdateAsync(Movie movie);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Movie>> AllAsync();
    Task<Movie?> GetAsync(Guid id);
    Task<Movie?> GetAsync(string slug);
}