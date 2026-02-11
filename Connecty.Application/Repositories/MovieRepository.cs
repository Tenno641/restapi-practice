using Connecty.Application.Models;

namespace Connecty.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = [];
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        _movies.Add(movie);
        
        return await Task.FromResult(true);
    }
    
    public async Task<bool> UpdateAsync(Guid id, Movie movie)
    {
        int existingMovie = _movies.FindIndex(m => m.Id == id);
        
        if (existingMovie == -1)
            return await Task.FromResult(false);

        _movies[existingMovie] = movie;

        return await Task.FromResult(true);
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        int deletedMovies = _movies.RemoveAll(movie => movie.Id == id);
        
        return await Task.FromResult(deletedMovies > 0);
    }
    
    public async Task<IEnumerable<Movie>> AllAsync()
    {
        return await Task.FromResult(_movies);
    }
    
    public async Task<Movie?> GetAsync(Guid id)
    {
        return await Task.FromResult(_movies.Find(movie => movie.Id == id));
    }
}