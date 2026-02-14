using System.Data;
using Connecty.Application.data;
using Connecty.Application.Models;
using Dapper;

namespace Connecty.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public MovieRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        using IDbTransaction transaction = connection.BeginTransaction();

        CommandDefinition command = new CommandDefinition($"""
                                       INSERT INTO "movies" (Id, Title, Slug, YearOfRelease)
                                       VALUES (@Id, @Title, @Slug, @YearOfRelease) 
                                       """, movie, transaction, cancellationToken: cancellationToken);

        int result = await connection.ExecuteAsync(command);

        if (result > 0)
        {
            string insertedGenres = string.Join(", ", movie.Genres.Select(genre => $"('{genre}', '{movie.Id}')"));

            if (!string.IsNullOrEmpty(insertedGenres))
            {

                CommandDefinition genreInsertionCommand = new CommandDefinition($"""
                                     INSERT INTO "genres" (Name, MovieId)
                                     VALUES {insertedGenres}
                                     """, transaction, cancellationToken: cancellationToken);

                await connection.ExecuteAsync(genreInsertionCommand);
            }
        }

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        using IDbTransaction transaction = connection.BeginTransaction();

        CommandDefinition deleteCommand = new CommandDefinition($"""
                                         DELETE FROM genres
                                         WHERE movieId = @MovieId;
                                         """, new { MovieId = movie.Id }, transaction, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(deleteCommand);

        string insertedGenres = string.Join(", ", movie.Genres.Select(genre => $"('{genre}', '{movie.Id}')"));
        CommandDefinition genreInsertionCommand = new CommandDefinition($"""
                                         INSERT INTO genres (name, movieId)
                                         VALUES {insertedGenres}
                                         """, transaction, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(genreInsertionCommand);

        CommandDefinition moviesInsertionCommand = new CommandDefinition($"""
                                      UPDATE movies SET
                                       title = @Title,
                                       slug = @Slug,
                                       yearofrelease = @YearOfRelease
                                       WHERE id = @Id                                  
                                      """, movie, transaction, cancellationToken: cancellationToken);

        int result = await connection.ExecuteAsync(moviesInsertionCommand);

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        using IDbTransaction transaction = connection.BeginTransaction();

        CommandDefinition genresDeletionCommand = new CommandDefinition($"""
                                     DELETE FROM genres 
                                     WHERE movieId = @id
                                     """, new { id }, transaction, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(genresDeletionCommand);

        CommandDefinition moviesDeletionCommand = new CommandDefinition($"""
                                    DELETE FROM movies 
                                    WHERE id = @id
                                    """, new { id }, transaction, cancellationToken: cancellationToken);

        int result = await connection.ExecuteAsync(moviesDeletionCommand);

        transaction.Commit();

        return result > 0;
    }

    public async Task<IEnumerable<Movie>> AllAsync(GetMoviesOptions options, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        string sortingCommand = string.Empty;

        if (options.SortBy is not null)
        {
            sortingCommand = $"""
                              , m.{options.SortBy}
                              ORDER BY m.{options.SortBy} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                              """;
        }

        CommandDefinition command = new CommandDefinition($"""
                                                           select m.*, 
                                                               string_agg(distinct g.name, ',') as genres , 
                                                               round(avg(r.rating), 1) as rating, 
                                                               myr.rating as userrating
                                                           from movies m 
                                                               left join genres g on m.id = g.movieid
                                                               left join ratings r on m.id = r.movieid
                                                               left join ratings myr on m.id = myr.movieid AND myr.userid = @userId
                                                           WHERE
                                                               (@title is null or title like ('%' || @title || '%')) AND
                                                               (@year is null or yearofrelease = @year)
                                                           group by id, userrating {sortingCommand}
                                                           """, new { options.UserId, options.Title, options.Year }, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync(command);
        
        return result.Select(entry => new Movie
        {
            Id = entry.id,
            Title = entry.title,
            YearOfRelease = entry.yearofrelease,
            Rating = (float?) entry.rating,
            UserRating = (int?) entry.userrating,
            Genres = Enumerable.ToList(entry.genres.Split(','))
        });
    }

    public async Task<Movie?> GetAsync(Guid id, CancellationToken cancellationToken, Guid? userId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition moviesRetrieveCommand = new CommandDefinition($"""
                                                           SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userRating 
                                                           FROM "movies" m
                                                           LEFT JOIN ratings r ON m.id = r.movieId
                                                           LEFT JOIN ratings myr ON myr.userid = @userId
                                                           WHERE id = @id
                                                           GROUP BY id, userRating;
                                                           """, new { id, userId }, cancellationToken: cancellationToken);

        Movie? movie = await connection.QuerySingleOrDefaultAsync<Movie>(moviesRetrieveCommand);

        if (movie is null) return null;

        CommandDefinition genresRetrieveCommand = new CommandDefinition($"""
                                                                         SELECT NAME FROM "genres"
                                                                         WHERE movieId = @id
                                                                         """, new { id }, cancellationToken: cancellationToken);

        IEnumerable<string> genres = await connection.QueryAsync<string>(genresRetrieveCommand);

        movie.Genres.AddRange(genres);

        return movie;
    }

    public async Task<Movie?> GetAsync(string slug, CancellationToken cancellationToken, Guid? userId)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition moviesRetrieveCommand = new CommandDefinition($"""
                                                           SELECT m.*, round(avg(r.rating), 1) as rating, myr.rating as userRating FROM "movies" m  
                                                           LEFT JOIN ratings r ON m.id = r.movieid
                                                           LEFT JOIN ratings myr ON myr.userid = @userId
                                                           WHERE slug = @slug
                                                           GROUP BY id, userRating;
                                                           """, new { slug, userId }, cancellationToken: cancellationToken);

        Movie? movie = await connection.QuerySingleOrDefaultAsync<Movie>(moviesRetrieveCommand);

        if (movie is null) return null;

        CommandDefinition genresRetrieveCommand = new CommandDefinition($"""
                                                                          SELECT NAME FROM "genres"
                                                                          WHERE movieId = @id
                                                                          """, new { movie.Id }, cancellationToken: cancellationToken);

        IEnumerable<string> genres = await connection.QueryAsync<string>(genresRetrieveCommand);

        movie.Genres.AddRange(genres);

        return movie;
    }
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT EXISTS (SELECT 1 FROM "movies"
                                                           WHERE id = @id)
                                                           """, new { id }, cancellationToken: cancellationToken);

        bool result = connection.ExecuteScalar<bool>(command);

        return result;
    }
}