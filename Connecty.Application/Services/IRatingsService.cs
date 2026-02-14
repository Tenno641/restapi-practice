namespace Connecty.Application.Services;

public interface IRatingsService
{
    Task<bool> RateMovieAsync(Guid movieId, int rating, Guid? userId = default, CancellationToken cancellationToken = default);
}