using Npgsql;

namespace WordGuessGame.Data;

public sealed class UserRepository
{
    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public bool UsernameExists(string username)
    {
        using var connection = _factory.OpenConnection();
        using var cmd = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM users WHERE lower(username) = lower(@u));",
            connection);
        cmd.Parameters.AddWithValue("u", username.Trim());
        return (bool)(cmd.ExecuteScalar() ?? false);
    }

    public int CreateUser(string username, string passwordHash)
    {
        using var connection = _factory.OpenConnection();
        using var cmd = new NpgsqlCommand(
            """
            INSERT INTO users (username, password_hash)
            VALUES (@u, @p)
            RETURNING id;
            """,
            connection);
        cmd.Parameters.AddWithValue("u", username.Trim());
        cmd.Parameters.AddWithValue("p", passwordHash);
        return Convert.ToInt32(cmd.ExecuteScalar() ?? throw new InvalidOperationException("Failed to create user."));
    }

    public (int Id, string Username, string PasswordHash)? FindByUsername(string username)
    {
        using var connection = _factory.OpenConnection();
        using var cmd = new NpgsqlCommand(
            """
            SELECT id, username, password_hash
            FROM users
            WHERE lower(username) = lower(@u)
            LIMIT 1;
            """,
            connection);
        cmd.Parameters.AddWithValue("u", username.Trim());

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return (
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2));
    }
}
