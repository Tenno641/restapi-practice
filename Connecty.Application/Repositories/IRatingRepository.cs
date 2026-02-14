using Connecty.Application.Models;

namespace Connecty.Application.Repositories;

public interface IRatingRepository
{
    Task<IEnumerable<MovieRating>> GetUserRatings(Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> DeleteRatingAsync(Guid movieId, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> RateMovieAsync(Guid movieId, int rating, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<(float? rating, int? userRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
}