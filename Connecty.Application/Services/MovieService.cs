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

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(movie, cancellationToken);

        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(movie, cancellationToken);

        bool exists = await _movieRepository.ExistsAsync(movie.Id, cancellationToken);

        if (!exists)
            return null;

        await _movieRepository.UpdateAsync(movie, cancellationToken);

        return movie;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _movieRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> AllAsync(CancellationToken cancellationToken)
    {
        return await _movieRepository.AllAsync(cancellationToken);
    }

    public async Task<Movie?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _movieRepository.GetAsync(id, cancellationToken);
    }

    public async Task<Movie?> GetAsync(string slug, CancellationToken cancellationToken)
    {
        return await _movieRepository.GetAsync(slug, cancellationToken);
    }
}