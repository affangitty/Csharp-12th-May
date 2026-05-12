using Npgsql;
using WordGuessGame.Models;

namespace WordGuessGame.Data;

public sealed class WordRepository
{
    private readonly DbConnectionFactory _factory;
    private readonly Random _rng;

    public WordRepository(DbConnectionFactory factory, Random rng)
    {
        _factory = factory;
        _rng = rng;
    }

    public string GetRandomWord(Difficulty difficulty)
    {
        using var connection = _factory.OpenConnection();
        using var countCmd = new NpgsqlCommand("SELECT COUNT(*)::int FROM words;", connection);
        var total = (int)(countCmd.ExecuteScalar() ?? 0);
        if (total == 0)
            throw new InvalidOperationException("No words in database. Ensure PostgreSQL is running and words were seeded.");

        var (offset, limit) = GetSlice(total, difficulty);

        using var selectCmd = new NpgsqlCommand(
            "SELECT word FROM words ORDER BY word OFFSET @o LIMIT @l;",
            connection);
        selectCmd.Parameters.AddWithValue("o", offset);
        selectCmd.Parameters.AddWithValue("l", limit);

        var slice = new List<string>();
        using (var reader = selectCmd.ExecuteReader())
        {
            while (reader.Read())
                slice.Add(reader.GetString(0).ToUpperInvariant());
        }

        if (slice.Count == 0)
            throw new InvalidOperationException("Word slice is empty.");

        return slice[_rng.Next(slice.Count)];
    }

    private static (int offset, int limit) GetSlice(int total, Difficulty difficulty)
    {
        if (total < 9)
            return (0, total);

        var third = total / 3;
        return difficulty switch
        {
            Difficulty.Easy => (0, third),
            Difficulty.Normal => (third, third),
            Difficulty.Hard => (third * 2, total - third * 2),
            _ => (0, total),
        };
    }
}
