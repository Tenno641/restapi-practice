using Connecty.Api.Mappings;
using Connecty.Application.Models;
using Connecty.Application.Repositories;
using Connecty.Contracts.Requests;
using Connecty.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Connecty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : Controller
{
    private readonly IMovieRepository _movieRepository;
    
    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateMovie request)
    {
        Movie movie = request.ToMovie();
        
        bool isCreated = await _movieRepository.CreateAsync(movie);

        if (!isCreated)
            return BadRequest();

        MovieResponse response = movie.ToResponse();

        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Movie>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<Movie> movies = await _movieRepository.AllAsync();

        MoviesResponse responses = movies.ToResponses();

        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id)
    {
        Movie? movie = await _movieRepository.GetAsync(id);

        if (movie is null)
            return NotFound();

        MovieResponse response = movie.ToResponse();
        
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Movie), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovie request)
    {
        Movie movie = request.ToMovie(id);
        
        bool isUpdated = await _movieRepository.UpdateAsync(id, movie);

        if (!isUpdated)
            return NotFound();

        MovieResponse response = movie.ToResponse();
        
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool isDeleted = await _movieRepository.DeleteAsync(id);

        if (!isDeleted)
            return NotFound();

        return Ok();
    }
}