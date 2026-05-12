using Npgsql;

namespace WordGuessGame.Data;

public sealed class DatabaseInitializer
{
    private readonly DbConnectionFactory _factory;

    public DatabaseInitializer(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void EnsureCreatedAndSeedWordsIfEmpty(string wordsFilePath)
    {
        using var connection = _factory.OpenConnection();
        using var batch = new NpgsqlBatch(connection);

        batch.BatchCommands.Add(new NpgsqlBatchCommand("""
            CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                username VARCHAR(64) NOT NULL UNIQUE,
                password_hash TEXT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            """));

        batch.BatchCommands.Add(new NpgsqlBatchCommand("""
            CREATE TABLE IF NOT EXISTS words (
                word VARCHAR(5) PRIMARY KEY
            );
            """));

        batch.BatchCommands.Add(new NpgsqlBatchCommand("""
            CREATE TABLE IF NOT EXISTS scores (
                id SERIAL PRIMARY KEY,
                user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                score INT NOT NULL,
                is_win BOOLEAN NOT NULL,
                attempts_used INT NOT NULL,
                difficulty SMALLINT NOT NULL,
                hidden_word VARCHAR(5) NOT NULL,
                played_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            """));

        batch.BatchCommands.Add(new NpgsqlBatchCommand("""
            CREATE INDEX IF NOT EXISTS ix_scores_user_played_at ON scores(user_id, played_at DESC);
            """));

        batch.ExecuteNonQuery();

        using var countCmd = new NpgsqlCommand("SELECT COUNT(*)::bigint FROM words;", connection);
        var wordCount = (long)(countCmd.ExecuteScalar() ?? 0L);
        if (wordCount > 0)
            return;

        if (!File.Exists(wordsFilePath))
            throw new FileNotFoundException($"Cannot seed words: file not found at '{wordsFilePath}'.", wordsFilePath);

        var words = File.ReadAllLines(wordsFilePath)
            .Select(x => x.Trim().ToUpperInvariant())
            .Where(x => x.Length == 5 && x.All(c => c is >= 'A' and <= 'Z'))
            .Distinct()
            .ToList();

        if (words.Count == 0)
            throw new InvalidOperationException("No valid 5-letter words found to seed the database.");

        using var tx = connection.BeginTransaction();
        using var insert = new NpgsqlCommand(
            "INSERT INTO words (word) VALUES (@w) ON CONFLICT (word) DO NOTHING;",
            connection,
            tx);

        foreach (var w in words)
        {
            insert.Parameters.Clear();
            insert.Parameters.AddWithValue("w", w);
            insert.ExecuteNonQuery();
        }

        tx.Commit();
    }
}
