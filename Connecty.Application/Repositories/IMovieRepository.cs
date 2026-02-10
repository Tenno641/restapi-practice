using Connecty.Application.Models;

namespace Connecty.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> Create(Movie movie);
    Task<bool> Update(Movie movie);
    Task<bool> Delete(Guid id);
    Task<IEnumerable<Movie>> All();
    Task<Movie?> Get(Guid id);
}