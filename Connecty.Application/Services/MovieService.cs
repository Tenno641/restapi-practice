using Connecty.Application.Models;
using Connecty.Application.Repositories;
using FluentValidation;

namespace Connecty.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _validator;
    
    public MovieService(IMovieRepository movieRepository, IValidator<Movie> validator)
    {
        _movieRepository = movieRepository;
        _validator = validator;
    }
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        
        return await _movieRepository.CreateAsync(movie);
    }
    
    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        
        bool exists = await _movieRepository.ExistsAsync(movie.Id);

        if (!exists)
            return null;
        
        await _movieRepository.UpdateAsync(movie);

        return movie;
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _movieRepository.DeleteAsync(id);
    }
    
    public async Task<IEnumerable<Movie>> AllAsync()
    {
        return await _movieRepository.AllAsync();
    }
    
    public async Task<Movie?> GetAsync(Guid id)
    {
        return await _movieRepository.GetAsync(id);
    }
    
    public async Task<Movie?> GetAsync(string slug)
    {
        return await _movieRepository.GetAsync(slug);
    }
}