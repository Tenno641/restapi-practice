using System.Data;
using Connecty.Application.data;
using Dapper;

namespace Connecty.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    
    public RatingRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                      SELECT round(avg(r.rate), 1) as rating
                                                      FROM ratings r 
                                                      WHERE movieid = @movieId
                                                      """, new { movieId }, cancellationToken: cancellationToken);

        float? rating = await connection.QuerySingleOrDefaultAsync<float>(command);

        return rating;
    }
    
    public async Task<(float? rating, int? userRating)> GetRatingAsync(Guid movieId, CancellationToken cancellationToken, Guid userId)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT round(avg(rate), 1), 
                                                           (SELECT rate FROM ratings WHERE ratings.movieid = @movieId AND userid = @userId LIMIT 1)
                                                           FROM ratings
                                                           WHERE movieid = @movieId
                                                           """, new { userId, movieId }, cancellationToken: cancellationToken);

        (float? rating, int? userrating) ratings = await connection.QuerySingleAsync<(float?, int?)>(command);

        return ratings;
    }
}