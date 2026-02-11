using Connecty.Application.Models;

namespace Connecty.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie);
    Task<bool> UpdateAsync(Guid id, Movie movie);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Movie>> AllAsync();
    Task<Movie?> GetAsync(Guid id);
}