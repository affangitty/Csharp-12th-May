using System.Text.RegularExpressions;
using WordGuessGame.Exceptions;

namespace WordGuessGame.Services;

public sealed partial class GuessValidator
{
    public string ValidateOrThrow(string? input)
    {
        var guess = (input ?? string.Empty).Trim().ToUpperInvariant();

        if (guess.Length == 0)
            throw new InvalidGuessException("Empty input. Please enter a 5-letter word.");

        if (guess.Length < 5)
            throw new InvalidGuessException("Input less than 5 letters. Please enter exactly 5 letters.");

        if (guess.Length > 5)
            throw new InvalidGuessException("Input greater than 5 letters. Please enter exactly 5 letters.");

        if (!LettersOnly().IsMatch(guess))
        {
            if (guess.Any(char.IsDigit))
                throw new InvalidGuessException("Input contains numbers. Use letters only (A-Z).");

            throw new InvalidGuessException("Input contains special characters. Use letters only (A-Z).");
        }

        return guess;
    }

    [GeneratedRegex("^[A-Z]{5}$")]
    private static partial Regex LettersOnly();
}

