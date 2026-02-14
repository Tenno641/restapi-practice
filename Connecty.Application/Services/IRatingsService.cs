using Connecty.Application.Models;

namespace Connecty.Application.Services;

public interface IRatingsService
{
    Task<IEnumerable<MovieRating>> GetUserRatings(Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> DeleteRatingAsync(Guid movieId, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> RateMovieAsync(Guid movieId, int rating, Guid? userId = default, CancellationToken cancellationToken = default);
}