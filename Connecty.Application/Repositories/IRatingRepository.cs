namespace Connecty.Application.Repositories;

public interface IRatingRepository
{
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken);
    Task<(float? rating, int? userRating)> GetRatingAsync(Guid movieId, CancellationToken cancellationToken, Guid userId);
}