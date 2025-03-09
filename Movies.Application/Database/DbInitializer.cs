using Dapper;

namespace Movies.Application.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFactory)
{
    public async Task InitializeAsync()
    {
        using var connection = await dbConnectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS Movies(
                id UUID PRIMARY KEY,
                slug TEXT NOT NULL,
                title TEXT NOT NULL,
                yearofrelease INTEGER NOT NULL
            )
            """);
        
        await connection.ExecuteAsync(
            """
            CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS movies_slug_idx
            on Movies
            USING BTREE(slug);
            """);
        
        await connection.ExecuteAsync(
            """
            CREATE TABLE IF NOT EXISTS Genres(
                movieId UUID REFERENCES Movies(id),
                name TEXT NOT NULL
            )
            """);
    }
}