using Asp.Versioning;
using Connecty.Api.Auth;
using Connecty.Api.Mappings;
using Connecty.Application.Models;
using Connecty.Application.Services;
using Connecty.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Connecty.Api.Controllers.V2;

[ApiController]
[ApiVersion(2.0)]
public class MoviesController : Controller
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromServices] LinkGenerator linkGenerator, string idOrSlug, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        
        Movie? movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetAsync(id, cancellationToken, userId)
            : await _movieService.GetAsync(idOrSlug, cancellationToken, userId);

        if (movie is null)
            return NotFound();

        MovieResponse response = movie.ToResponse();
        
        response.Links.Add(new Link
        {
            Type = HttpMethods.Put,
            href = linkGenerator.GetPathByAction(HttpContext, "Create", values: new { id = movie.Id }),
            Rel = "Self"
        });
        
        response.Links.Add(new Link
        {
            Type = HttpMethods.Delete,
            href = linkGenerator.GetPathByAction(HttpContext, "Delete",values: new { id = movie.Id }),
            Rel = "Self"
        });

        return Ok(response);
    }
}