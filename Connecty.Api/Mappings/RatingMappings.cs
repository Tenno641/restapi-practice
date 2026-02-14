using Connecty.Application.Models;
using Connecty.Contracts.Responses;

namespace Connecty.Api.Mappings;

public static class RatingMappings
{
    public static RatingResponse ToMovieResponse(this MovieRating movieRating)
    {
        RatingResponse ratingResponse = new RatingResponse
        {
            MovieId = movieRating.MovieId,
            Rating = movieRating.Rating,
            Slug = movieRating.Slug
        };

        return ratingResponse;
    }
}