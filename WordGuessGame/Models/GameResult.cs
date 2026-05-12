namespace WordGuessGame.Models;

public sealed class GameResult
{
    public required string HiddenWord { get; init; }
    public required Difficulty Difficulty { get; init; }
    public required bool IsWin { get; init; }
    public required int AttemptsUsed { get; init; }
    public required int MaxAttempts { get; init; }
    public required int Score { get; init; }
}

