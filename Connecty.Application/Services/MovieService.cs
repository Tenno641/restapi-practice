using System.Text.Json;
using Connecty.Application.Models;
using Connecty.Application.Repositories;
using FluentValidation;

namespace Connecty.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _validator;
    private readonly IRatingRepository _ratingRepository;

    public MovieService(IMovieRepository movieRepository, IValidator<Movie> validator, IRatingRepository ratingRepository)
    {
        _movieRepository = movieRepository;
        _validator = validator;
        _ratingRepository = ratingRepository;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(movie, cancellationToken);

        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(movie, cancellationToken);

        bool exists = await _movieRepository.ExistsAsync(movie.Id, cancellationToken);

        if (!exists)
            return null;

        await _movieRepository.UpdateAsync(movie, cancellationToken);

        if (!userId.HasValue)
        {
            float? rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
            return movie;
        }

        (float? rating, int? userRating) ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
        movie.UserRating = ratings.userRating;
        movie.Rating = ratings.rating;
        
        return movie;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _movieRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> AllAsync(CancellationToken cancellationToken, Guid? userId)
    {
        return await _movieRepository.AllAsync(cancellationToken, userId);
    }

    public async Task<Movie?> GetAsync(Guid id, CancellationToken cancellationToken, Guid? userId)
    {
        return await _movieRepository.GetAsync(id, cancellationToken, userId);
    }

    public async Task<Movie?> GetAsync(string slug, CancellationToken cancellationToken, Guid? userId)
    {
        return await _movieRepository.GetAsync(slug, cancellationToken, userId);
    }
}