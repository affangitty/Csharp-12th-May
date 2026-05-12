using WordGuessGame.Models;
using WordGuessGame.Services;

namespace WordGuessGame;

public sealed class AuthConsoleFlow
{
    private readonly AuthService _auth;

    public AuthConsoleFlow(AuthService auth)
    {
        _auth = auth;
    }

    public LoggedInPlayer? RunUntilLoginOrExit()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== WORD GUESS GAME ===");
            Console.WriteLine("Sign in to play.\n");
            Console.WriteLine("1) Register");
            Console.WriteLine("2) Login");
            Console.WriteLine("3) Exit");
            Console.Write("\nChoice (1-3): ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RegisterFlow();
                    break;
                case "2":
                    var player = LoginFlow();
                    if (player is not null)
                        return player;
                    break;
                case "3":
                    return null;
                default:
                    WriteError("Invalid choice.");
                    PressEnter();
                    break;
            }
        }
    }

    private void RegisterFlow()
    {
        Console.WriteLine("--- Register ---");
        Console.Write("Username: ");
        var username = Console.ReadLine() ?? string.Empty;

        var mask = PromptYesNo("Mask password while typing?");
        Console.Write("Password: ");
        var password = mask ? SecureConsole.ReadLineMasked() : ReadPasswordPlain();

        Console.Write("Confirm password: ");
        var confirm = mask ? SecureConsole.ReadLineMasked() : ReadPasswordPlain();

        if (!string.Equals(password, confirm, StringComparison.Ordinal))
        {
            WriteError("Passwords do not match.");
            PressEnter();
            return;
        }

        try
        {
            _auth.Register(username, password);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nRegistration successful. You can log in from the main menu.");
            Console.ResetColor();
        }
        catch (InvalidOperationException ex)
        {
            WriteError(ex.Message);
        }
        catch (Exception ex)
        {
            WriteError($"Registration failed: {ex.Message}");
        }

        PressEnter();
    }

    private LoggedInPlayer? LoginFlow()
    {
        Console.WriteLine("--- Login ---");
        Console.Write("Username: ");
        var username = Console.ReadLine() ?? string.Empty;

        var mask = PromptYesNo("Mask password while typing?");
        Console.Write("Password: ");
        var password = mask ? SecureConsole.ReadLineMasked() : ReadPasswordPlain();

        var player = _auth.TryLogin(username, password);
        if (player is null)
        {
            WriteError("Invalid username or password.");
            PressEnter();
            return null;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nWelcome, {player.Username}!");
        Console.ResetColor();
        PressEnter();
        return player;
    }

    private static string ReadPasswordPlain()
    {
        return Console.ReadLine() ?? string.Empty;
    }

    private static bool PromptYesNo(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (Y/N): ");
            var raw = Console.ReadLine();
            if (raw is null)
                return true;

            var v = raw.Trim();
            if (v.Equals("Y", StringComparison.OrdinalIgnoreCase))
                return true;
            if (v.Equals("N", StringComparison.OrdinalIgnoreCase))
                return false;

            WriteError("Please enter Y or N.");
        }
    }

    private static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void PressEnter()
    {
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
}
