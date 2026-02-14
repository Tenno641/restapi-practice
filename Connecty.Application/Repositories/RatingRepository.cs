using System.Data;
using Connecty.Application.data;
using Connecty.Application.Models;
using Dapper;

namespace Connecty.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    
    public RatingRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<MovieRating>> GetUserRatings(Guid? userId, CancellationToken cancellationToken = default)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT r.rating, r.movieId, m.slug
                                                           FROM ratings r 
                                                           INNER JOIN movies m ON r.movieId = m.id
                                                           WHERE userid = @userId
                                                           """, new { userId }, cancellationToken: cancellationToken);

        IEnumerable<MovieRating> movieRatings = await connection.QueryAsync<MovieRating>(command);

        return movieRatings;
    }
    
    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           DELETE FROM ratings
                                                           WHERE movieid = @movieId and userid = @userId
                                                           """, new { movieId, userId}, cancellationToken: cancellationToken);

        int result = await connection.ExecuteAsync(command);

        return result > 0;
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