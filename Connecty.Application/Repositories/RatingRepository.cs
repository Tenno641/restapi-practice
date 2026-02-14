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

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid? userId, CancellationToken cancellationToken = default)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        
        CommandDefinition command = new CommandDefinition($"""
                                                           INSERT INTO ratings (userid, movieid, rating)
                                                           VALUES (@userId, @movieId, @rating)
                                                           ON CONFLICT (movieid, userid) DO UPDATE SET rating = @rating
                                                           """, new { movieId, userId, rating }, cancellationToken: cancellationToken);

        int result = await connection.ExecuteAsync(command);

        return result > 0;
    }
    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                      SELECT round(avg(r.rating), 1) as rating
                                                      FROM ratings r 
                                                      WHERE movieid = @movieId
                                                      """, new { movieId }, cancellationToken: cancellationToken);

        float? rating = await connection.QuerySingleOrDefaultAsync<float>(command);

        return rating;
    }

    public async Task<(float? rating, int? userRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT round(avg(rating), 1), 
                                                           (SELECT rating FROM ratings WHERE ratings.movieid = @movieId AND userid = @userId LIMIT 1)
                                                           FROM ratings
                                                           WHERE movieid = @movieId
                                                           """, new { userId, movieId }, cancellationToken: cancellationToken);

        (float? rating, int? userrating) ratings = await connection.QuerySingleAsync<(float?, int?)>(command);

        return ratings;
    }
}