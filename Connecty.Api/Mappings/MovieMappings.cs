using Connecty.Application.Models;
using Connecty.Contracts.Requests;
using Connecty.Contracts.Responses;

namespace Connecty.Api.Mappings;

public static class MovieMappings
{
    public static Movie ToMovie(this CreateMovie request)
    {
        Movie movie = new Movie
        {
            Id = Guid.CreateVersion7(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres
        };

        return movie;
    }

    public static MovieResponse ToResponse(this Movie movie)
    {
        MovieResponse response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            YearOfRelease = movie.YearOfRelease,
            Genres = movie.Genres
        };

        return response;
    }

    public static MoviesResponse ToResponses(this IEnumerable<Movie> movies)
    {
        IEnumerable<MovieResponse> moviesResponse = movies.Select(ToResponse);

        return new MoviesResponse { Items= moviesResponse };
    }

    public static Movie ToMovie(this UpdateMovie request, Guid id)
    {
        Movie movie = new Movie
        {
            Id = id,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres
        };

        return movie;
    }
}