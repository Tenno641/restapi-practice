using Asp.Versioning;
using Connecty.Api.Auth;
using Connecty.Api.Mappings;
using Connecty.Application.Models;
using Connecty.Application.Services;
using Connecty.Contracts.Requests;
using Connecty.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connecty.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
public class RatingsController : Controller
{
    private readonly IRatingsService _ratingsService;

    public RatingsController(IRatingsService ratingsService)
    {
        _ratingsService = ratingsService;
    }

    [Authorize]
    [HttpPost(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromRoute] Guid id, [FromBody] RateRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();

        bool isRated = await _ratingsService.RateMovieAsync(id, request.Rating, userId, cancellationToken);

        return isRated
            ? NoContent()
            : NotFound();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();

        bool isDeleted = await _ratingsService.DeleteRatingAsync(id, userId, cancellationToken);

        return isDeleted
            ? NoContent()
            : NotFound();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.UserRatings)]
    [ProducesResponseType(typeof(List<RatingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRatings(CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();

        IEnumerable<MovieRating> movieRatings = await _ratingsService.GetUserRatings(userId, cancellationToken);

        IEnumerable<RatingResponse> ratingResponses = movieRatings.Select(movieRating => movieRating.ToMovieResponse());

        return Ok(ratingResponses);
    }
}