using Connecty.Contracts.Requests;
using Connecty.Contracts.Responses;
using Refit;

namespace Connecty.Sdk;

[Headers("Authorization: Bearer")]
public interface IMovieApi
{
    [Get(ApiEndpoints.Movies.Get)]
    Task<MovieResponse> GetMovieAsync(string idOrSlug);
    [Get(ApiEndpoints.Movies.GetAll)]
    Task<MoviesResponse> GetMoviesAsync(GetMoviesOptionsRequest optionsRequest);
    [Post(ApiEndpoints.Movies.Create)]
    Task<MovieResponse> CreateMovieAsync(CreateMovie createMovieRequest);
}