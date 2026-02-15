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
public class MoviesController : Controller
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedMember)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateMovie request, CancellationToken cancellationToken)
    {
        Movie movie = request.ToMovie();

        bool isCreated = await _movieService.CreateAsync(movie, cancellationToken);

        if (!isCreated)
            return BadRequest();

        MovieResponse response = movie.ToResponse();

        return CreatedAtAction(nameof(Get), new { idOrSlug = response.Id }, response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetMoviesOptionsRequest options, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();

        GetMoviesOptions moviesOptions = options.ToMoviesOptions()
            .WithUserId(userId);
        
        IEnumerable<Movie> movies = await _movieService.AllAsync(moviesOptions, cancellationToken);

        int total = await _movieService.GetTotalCountAsync(options.Title, options.Year, cancellationToken);

        MoviesResponse responses = movies.ToResponses(options.Page, options.PageSize, total);

        return Ok(responses);
    }

    [ApiVersion(1.0)]
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

        return Ok(response);
    }

    [Authorize(AuthConstants.TrustedMember)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovie request, CancellationToken cancellationToken)
    {
        Guid? userId = HttpContext.GetUserId();
        
        Movie movie = request.ToMovie(id);

        Movie? updatedMovie = await _movieService.UpdateAsync(movie, userId, cancellationToken);

        if (updatedMovie is null)
            return NotFound();

        MovieResponse response = movie.ToResponse();

        return Ok(response);
    }

    [Authorize(AuthConstants.AdminPolicy)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        bool isDeleted = await _movieService.DeleteAsync(id, cancellationToken);

        if (!isDeleted)
            return NotFound();

        return Ok();
    }
}