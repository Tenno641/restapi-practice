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
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        using IDbTransaction transaction = connection.BeginTransaction();

        int result = await connection.ExecuteAsync($"""
                                      INSERT INTO "movies" (Id, Title, Slug, YearOfRelease)
                                      VALUES (@Id, @Title, @Slug, @YearOfRelease) 
                                      """, movie);

        if (result > 0)
        {
            foreach (string genre in movie.Genres)
            {
                
                await connection.ExecuteAsync($"""
                                       INSERT INTO "genres" (Name, MovieId)
                                       VALUES (@Name, @MovieId)
                                       """, new {Name = genre, MovieId = movie.Id });
            }
        }
        
        transaction.Commit();

        return result > 0;
    }
    
    public async Task<bool> UpdateAsync(Movie movie)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        using IDbTransaction transaction = connection.BeginTransaction();

        await connection.ExecuteAsync($"""
                                              DELETE FROM genres
                                              WHERE movieId = @MovieId
                                              """, new { MovieId = movie.Id });

        foreach (string genre in movie.Genres)
        {
            await connection.ExecuteAsync($"""
                                           INSERT INTO genres (name, movieId)
                                           VALUES (@Name, @MovieId) 
                                           """, new { Name = genre, MovieId = movie.Id });
        }

        int result = await connection.ExecuteAsync($"""
                                       UPDATE movies SET 
                                       title = @Title,
                                       slug = @Slug,
                                       yearofrelease = @YearOfRelease
                                       WHERE id = @Id
                                       """, movie);

        transaction.Commit();

        return result > 0;
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        using IDbTransaction transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync($"""
                                       DELETE FROM genres 
                                       WHERE movieId = @id
                                       """, new { id });

        int result = await connection.ExecuteAsync($"""
                                       DELETE FROM movies 
                                       WHERE id = @id
                                       """, new { id });
       
        transaction.Commit();

        return result > 0;
    }
    
    public async Task<IEnumerable<Movie>> AllAsync()
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT movies.*, string_agg(genres.Name, ',') as genres
                                                           FROM movies RIGHT JOIN genres ON movies.Id = genres.movieId
                                                           GROUP BY id;
                                                           """);

        var result = (await connection.QueryAsync(command)).ToList();

        IEnumerable<Movie> movies = result.Select(entry => new Movie
        {
            Id = entry.id,
            Title = entry.title,
            YearOfRelease = entry.yearofrelease,
            Genres = (entry.genres as string)?.Split(',').ToList() ?? Enumerable.Empty<string>().ToList()
        });

        return movies;
    }
    
    public async Task<Movie?> GetAsync(Guid id)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition moviesRetrieveCommand = new CommandDefinition($"""
                                                           SELECT * FROM "movies"
                                                           WHERE id = @id
                                                           """, new { id });

        Movie? movie = await connection.QuerySingleOrDefaultAsync<Movie>(moviesRetrieveCommand);

        if (movie is null) return null;

        CommandDefinition genresRetrieveCommand = new CommandDefinition($"""
                                                                         SELECT NAME FROM "genres"
                                                                         WHERE movieId = @id
                                                                         """, new { id });
        
        IEnumerable<string> genres = await connection.QueryAsync<string>(genresRetrieveCommand);

        movie.Genres.AddRange(genres);

        return movie;
    }
    
    public async Task<Movie?> GetAsync(string slug)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
     
         CommandDefinition moviesRetrieveCommand = new CommandDefinition($"""
                                                            SELECT * FROM "movies"
                                                            WHERE slug = @slug
                                                            """, new { slug });
 
         Movie? movie = await connection.QuerySingleOrDefaultAsync<Movie>(moviesRetrieveCommand);
 
         if (movie is null) return null;
 
         CommandDefinition genresRetrieveCommand = new CommandDefinition($"""
                                                                          SELECT NAME FROM "genres"
                                                                          WHERE movieId = @id
                                                                          """, new { movie.Id });
         
         IEnumerable<string> genres = await connection.QueryAsync<string>(genresRetrieveCommand);
 
         movie.Genres.AddRange(genres);
 
         return movie;
    }
    public async Task<bool> ExistsAsync(Guid id)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        
        CommandDefinition command = new CommandDefinition($"""
                                                           SELECT COUNT(*) FROM "movies"
                                                           WHERE id = @id
                                                           """, id);

        bool result = connection.ExecuteScalar<bool>(command);

        return result;
    }
}