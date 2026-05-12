namespace WordGuessGame.Models;

public sealed class Feedback
{
    public required string Guess { get; init; }
    public required string Pattern { get; init; } // 'G', 'Y', 'X'
}

