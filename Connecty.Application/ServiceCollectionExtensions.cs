using System.Reflection;
using Connecty.Application.data;
using Connecty.Application.Repositories;
using Connecty.Application.Services;
using Connecty.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Connecty.Application;

public static class ServiceCollectionExtensions 
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository, MovieRepository>();
        services.AddSingleton<IMovieService, MovieService>();
        services.AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(MovieValidator)), ServiceLifetime.Singleton);
        
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>(_ => new DatabaseConnectionFactory(connectionString));
        
        return services;
    }
}