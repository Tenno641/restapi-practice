using Connecty.Application.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace Connecty.Application.Services;

public class RatingsService : IRatingsService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMovieRepository _movieRepository;
    
    public RatingsService(IRatingRepository ratingRepository, IMovieRepository movieRepository)
    {
        _ratingRepository = ratingRepository;
        _movieRepository = movieRepository;
    }
    
    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid? userId, CancellationToken cancellationToken = default)
    {
        if (rating is < 0 or > 5)
        {
            throw new ValidationException(new []
            {
                new ValidationFailure(nameof(rating), "Rating must be 0..5")
            });
        }

        bool movieExists = await _movieRepository.ExistsAsync(movieId, cancellationToken);
        if (!movieExists)
            return false;

        bool isRated = await _ratingRepository.RateMovieAsync(movieId, rating, userId, cancellationToken);

        return isRated;
    }
}