using Connecty.Application.Models;
using Connecty.Application.Repositories;
using FluentValidation;

namespace Connecty.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;
    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;

        RuleFor(movie => movie.Id)
            .NotEmpty();

        RuleFor(movie => movie.Title)
            .NotEmpty();

        RuleFor(movie => movie.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(movie => movie.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("Slug already exists");
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken cancellationToken)
    {
        Movie? existingMovie = await _movieRepository.GetAsync(slug);

        if (existingMovie is not null)
            return existingMovie.Id == movie.Id;

        return true;
    }
}