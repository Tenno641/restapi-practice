using Connecty.Application.Models;
using FluentValidation;

namespace Connecty.Application.Validators;

public class GetMoviesOptionsValidator : AbstractValidator<GetMoviesOptions>
{
    public GetMoviesOptionsValidator()
    {
        string[] validSortingFields = ["title", "yearofrelease"];

        RuleFor(options => options.SortBy)
            .Must(sortBy => sortBy is null || validSortingFields.Contains(sortBy, StringComparer.InvariantCultureIgnoreCase))
            .WithMessage("SortBy must be 'title' or 'yearofrelease'");

        RuleFor(options => options.Year)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);
    }
}