using System.Text.Json;
using Connecty.Contracts.Requests;
using Connecty.Contracts.Responses;
using Connecty.Sdk;
using Connecty.Sdk.Consumer;
using Microsoft.Extensions.DependencyInjection;
using Refit;

IServiceCollection services = new ServiceCollection();

services.AddHttpClient();

services.AddSingleton<AuthTokenService>();

services.AddRefitClient<IMovieApi>(provider => new RefitSettings
    {
        AuthorizationHeaderValueGetter =  async (_, _) => await provider.GetRequiredService<AuthTokenService>().GenerateTokenAsync()
    })
    .ConfigureHttpClient(options =>
    {
        options.BaseAddress = new Uri("https://localhost:5001");
    });

IServiceProvider serviceProvider = services.BuildServiceProvider();

IMovieApi client = serviceProvider.GetRequiredService<IMovieApi>();

try
{
    MovieResponse movie = await client.GetMovieAsync("019c5b16-c416-7d77-8bfb-12469233395d");
    Console.WriteLine(JsonSerializer.Serialize(movie));

    GetMoviesOptionsRequest options = new GetMoviesOptionsRequest
    {
        Page = 1,
        PageSize = 10,
    };
    MoviesResponse movies = await client.GetMoviesAsync(options);
    Console.WriteLine(JsonSerializer.Serialize(movies));

    CreateMovie createMovieRequest = new CreateMovie
    {
        Title = "Bring Her Back",
        YearOfRelease = 2015,
        Genres = ["Horror"]
    };
    MovieResponse movieResponse = await client.CreateMovieAsync(createMovieRequest);
    Console.WriteLine(JsonSerializer.Serialize(movieResponse));

}
catch (ValidationApiException apiException)
{
    Console.WriteLine(apiException.Message);
}