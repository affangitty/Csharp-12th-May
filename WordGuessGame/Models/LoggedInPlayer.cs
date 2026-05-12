namespace WordGuessGame.Models;

public sealed class LoggedInPlayer
{
    public required int UserId { get; init; }
    public required string Username { get; init; }
}
