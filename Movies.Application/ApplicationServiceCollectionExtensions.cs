using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Application.Validators;

namespace Movies.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<Repositories.IMovieRepository, Repositories.MovieRepository>();
        serviceCollection.AddSingleton<IMovieService, MovieService>();

        serviceCollection.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
        
        return serviceCollection;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection serviceCollection, string connectionString)
    {
        serviceCollection.AddSingleton<IDbConnectionFactory>(_ =>
            new NpgsqlConnectionFactory(connectionString)
        );

        serviceCollection.AddSingleton<DbInitializer>();
        
        return serviceCollection;
    }
}