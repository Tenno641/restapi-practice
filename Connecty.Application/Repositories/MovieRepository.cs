using Connecty.Application.Models;

namespace Connecty.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = [];
    
    public Task<bool> Create(Movie movie)
    {
        _movies.Add(movie);
        
        return Task.FromResult(true);
    }
    
    public Task<bool> Update(Movie movie)
    {
        int existingMovie = _movies.FindIndex(m => m.Id == movie.Id);
        
        if (existingMovie == -1)
            return Task.FromResult(false);

        _movies[existingMovie] = movie;

        return Task.FromResult(true);
    }
    
    public Task<bool> Delete(Guid id)
    {
        int deletedMovies = _movies.RemoveAll(movie => movie.Id == id);
        
        return Task.FromResult(deletedMovies > 0);
    }
    
    public async Task<IEnumerable<Movie>> All()
    {
        return await Task.FromResult(_movies);
    }
    
    public async Task<Movie?> Get(Guid id)
    {
        return await Task.FromResult(_movies.Find(movie => movie.Id == id));
    }
}