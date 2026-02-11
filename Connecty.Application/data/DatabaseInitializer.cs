using System.Data;
using Dapper;

namespace Connecty.Application.data;

public class DatabaseInitializer
{
    private readonly IDbConnection _connection;
    
    public DatabaseInitializer(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task InitializeAsync()
    {
        await _connection.ExecuteAsync($"""
                 CREATE TABLE IF NOT EXISTS movies (
                     Id UUID PRIMARY key,
                     Title TEXT NOT NULL,
                     Slug TEXT NOT NULL,
                     YearOfRelease integer NOT NULL
                     );
             """);

        await _connection.ExecuteAsync($"""
                CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_idx
                on movies
                using btree(slug);
            """);

        await _connection.ExecuteAsync($"""
               CREATE TABLE IF NOT EXISTS genres(
                   Name TEXT NOT NULL,
                   MovieId UUID REFERENCES movies (id) 
                );
           """);
    }
}