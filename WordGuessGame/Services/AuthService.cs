using WordGuessGame.Data;
using WordGuessGame.Models;

namespace WordGuessGame.Services;

public sealed class AuthService
{
    private const int MinPasswordLength = 6;
    private const int MaxUsernameLength = 64;

    private readonly UserRepository _users;

    public AuthService(UserRepository users)
    {
        _users = users;
    }

    public string? ValidateNewUsername(string username)
    {
        var u = (username ?? string.Empty).Trim();
        if (u.Length < 3)
            return "Username must be at least 3 characters.";
        if (u.Length > MaxUsernameLength)
            return $"Username must be at most {MaxUsernameLength} characters.";
        if (_users.UsernameExists(u))
            return "That username is already taken.";
        return null;
    }

    public string? ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            return $"Password must be at least {MinPasswordLength} characters.";
        return null;
    }

    public void Register(string username, string password)
    {
        var userError = ValidateNewUsername(username);
        if (userError is not null)
            throw new InvalidOperationException(userError);

        var passError = ValidatePassword(password);
        if (passError is not null)
            throw new InvalidOperationException(passError);

        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        _users.CreateUser(username, hash);
    }

    public LoggedInPlayer? TryLogin(string username, string password)
    {
        var row = _users.FindByUsername(username);
        if (row is null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, row.Value.PasswordHash))
            return null;

        return new LoggedInPlayer { UserId = row.Value.Id, Username = row.Value.Username };
    }
}
