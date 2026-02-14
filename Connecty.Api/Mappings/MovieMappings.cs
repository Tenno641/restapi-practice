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
            Genres = request.Genres.ToList()
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
            Slug = movie.Slug,
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            Genres = movie.Genres
        };

        return response;
    }

    public static MoviesResponse ToResponses(this IEnumerable<Movie> movies, int? page, int? pageSize, int total)
    {
        IEnumerable<MovieResponse> items = movies.Select(ToResponse);

        MoviesResponse moviesResponse = new MoviesResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total,
            Readable = total > page * pageSize
        };

        return moviesResponse;
    }

    public static Movie ToMovie(this UpdateMovie request, Guid id)
    {
        Movie movie = new Movie
        {
            Id = id,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };

        return movie;
    }

    public static GetMoviesOptions ToMoviesOptions(this GetMoviesOptionsRequest request)
    {
        GetMoviesOptions moviesOptions = new GetMoviesOptions
        {
            Title = request.Title,
            Year = request.Year,
            SortBy = request.SortBy?.Trim('+', '-'),
            SortOrder = request.SortBy is null ? SortOrder.Unsorted
                : request.SortBy.StartsWith('-') ? SortOrder.Descending : SortOrder.Ascending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return moviesOptions;
    }

    public static GetMoviesOptions WithUserId(this GetMoviesOptions options, Guid? userId)
    {
        options.UserId = userId;
        return options;
    }
}