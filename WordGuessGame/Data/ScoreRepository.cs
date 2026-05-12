using Npgsql;
using WordGuessGame.Models;

namespace WordGuessGame.Data;

public sealed class ScoreRepository
{
    private readonly DbConnectionFactory _factory;

    public ScoreRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Insert(int userId, GameResult result)
    {
        using var connection = _factory.OpenConnection();
        using var cmd = new NpgsqlCommand(
            """
            INSERT INTO scores (user_id, score, is_win, attempts_used, difficulty, hidden_word)
            VALUES (@uid, @score, @win, @attempts, @diff, @word);
            """,
            connection);

        cmd.Parameters.AddWithValue("uid", userId);
        cmd.Parameters.AddWithValue("score", result.Score);
        cmd.Parameters.AddWithValue("win", result.IsWin);
        cmd.Parameters.AddWithValue("attempts", result.AttemptsUsed);
        cmd.Parameters.AddWithValue("diff", (short)result.Difficulty);
        cmd.Parameters.AddWithValue("word", result.HiddenWord.ToUpperInvariant());

        cmd.ExecuteNonQuery();
    }
}
