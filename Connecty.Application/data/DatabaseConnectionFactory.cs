using System.Data;
using Npgsql;

namespace Connecty.Application.data;

public interface IDatabaseConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}