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
        throw new NotImplementedException();
    }
    
    public async Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
    
    public async Task<IEnumerable<Movie>> AllAsync()
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        CommandDefinition command = new CommandDefinition("SELECT * FROM public.Movies");

        IEnumerable<Movie> result = (await connection.QueryAsync<Movie>(command)).ToList();

        return result;
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
        throw new NotImplementedException();
    }
}