using Connecty.Api.Mappings;
using Connecty.Application.Models;
using Connecty.Application.Services;
using Connecty.Contracts.Requests;
using Connecty.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Connecty.Api.Controllers;

[ApiController]
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
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        IEnumerable<Movie> movies = await _movieService.AllAsync(cancellationToken);

        MoviesResponse responses = movies.ToResponses();

        return Ok(responses);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string idOrSlug, CancellationToken cancellationToken)
    {
        Movie? movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetAsync(id, cancellationToken)
            : await _movieService.GetAsync(idOrSlug, cancellationToken);

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
        Movie movie = request.ToMovie(id);

        Movie? updatedMovie = await _movieService.UpdateAsync(movie, cancellationToken);

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