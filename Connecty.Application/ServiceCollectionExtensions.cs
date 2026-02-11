using Connecty.Application.data;
using Connecty.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Connecty.Application;

public static class ServiceCollectionExtensions 
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository, MovieRepository>();
        
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>(_ => new DatabaseConnectionFactory(connectionString));
        
        return services;
    }
}