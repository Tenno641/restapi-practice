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

    public async Task<IEnumerable<Movie>> AllAsync(CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT movies.*, string_agg(genres.Name, ',') as genres
                                                           FROM movies RIGHT JOIN genres ON movies.Id = genres.movieId
                                                           GROUP BY id;
                                                           """, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync(command);

        IEnumerable<Movie> movies = result.Select(entry => new Movie
        {
            Id = entry.id,
            Title = entry.title,
            YearOfRelease = entry.yearofrelease,
            Genres = (entry.genres as string)?.Split(',').ToList() ?? Enumerable.Empty<string>().ToList()
        });

        return movies;
    }

    public async Task<Movie?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition moviesRetrieveCommand = new CommandDefinition($"""
                                                           SELECT * FROM "movies"
                                                           WHERE id = @id
                                                           """, new { id }, cancellationToken: cancellationToken);

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

    public async Task<Movie?> GetAsync(string slug, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition moviesRetrieveCommand = new CommandDefinition($"""
                                                            SELECT * FROM "movies"
                                                            WHERE slug = @slug
                                                            """, new { slug }, cancellationToken: cancellationToken);

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