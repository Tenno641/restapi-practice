using Connecty.Api.Auth;
using Connecty.Application.Services;
using Connecty.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Connecty.Api.Controllers;

[ApiController]
public class RatingsController : Controller
{
    private readonly IRatingsService _ratingsService;

    public RatingsController(IRatingsService ratingsService)
    {
        _ratingsService = ratingsService;
    }

    [HttpPost(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Rate([FromRoute] Guid id, [FromBody] RateRequest request, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();

        bool isRated = await _ratingsService.RateMovieAsync(id, request.Rating, userId, cancellationToken);

        return isRated
            ? NoContent()
            : NotFound();
    }
}