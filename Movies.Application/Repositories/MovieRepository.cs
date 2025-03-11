﻿using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO Movies (id, slug, title, yearofrelease)
            VALUES (@Id, @Slug, @Title, @YearOfRelease)
            """, movie, cancellationToken: token
        ));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                    """
                    INSERT INTO Genres (movieId, name)
                    VALUES (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token
                ));
            }
        }
        
        transaction.Commit();

        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                    SELECT * FROM movies WHERE id = @Id 
                """, new { Id = id }, cancellationToken: token
                )
            );

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition
            (
                """
                    SELECT name FROM genres WHERE movieid = @Id
                """, new { Id = id }, cancellationToken: token
                )
            );

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                    SELECT * FROM movies WHERE slug = @Slug 
                """, new { Slug = slug }, cancellationToken: token
                )
            );

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition
            (
                """
                    SELECT name FROM genres WHERE movieid = @Id
                """, new { Id = movie.Id }, cancellationToken: token
                )
            );

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        var movies = await connection.QueryAsync(
            new CommandDefinition(
                """
                    SELECT M.*, STRING_AGG(G.name, ',') AS genres
                    FROM movies M LEFT JOIN genres G ON M.id = G.movieid
                    GROUP BY M.id
                """, cancellationToken: token
            ));

        return movies.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(","))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
                DELETE FROM genres WHERE movieid = @Id
            """, new { Id = movie.Id }, cancellationToken: token
            ));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                    INSERT INTO genres (movieid, name)
                    VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token
            ));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
                UPDATE movies SET slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                WHERE id = @Id
            """, new {  movie.Id, movie.Slug,  movie.Title, movie.YearOfRelease }, cancellationToken: token
        ));
        
        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();
        
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
                DELETE FROM genres WHERE movieid = @Id
            """, new { Id = id }, cancellationToken: token
            ));
        
        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
                DELETE FROM movies WHERE id = @Id
            """, new { Id = id }, cancellationToken: token
            ));
        
        transaction.Commit();
        
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            """
                SELECT COUNT(1) FROM movies WHERE id = @id
            """, new { Id = id }, cancellationToken: token
            ));
    }
}