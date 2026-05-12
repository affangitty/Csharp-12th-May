using WordGuessGame.Models;

namespace WordGuessGame.Services;

public sealed class FeedbackGenerator
{
    public Feedback Generate(string hiddenWord, string guess)
    {
        var hidden = hiddenWord.ToUpperInvariant();
        var g = guess.ToUpperInvariant();

        var pattern = new char[5];
        var remaining = new Dictionary<char, int>();

        for (var i = 0; i < 5; i++)
        {
            if (g[i] == hidden[i])
            {
                pattern[i] = 'G';
            }
            else
            {
                var c = hidden[i];
                remaining[c] = remaining.TryGetValue(c, out var count) ? count + 1 : 1;
            }
        }

        for (var i = 0; i < 5; i++)
        {
            if (pattern[i] == 'G')
                continue;

            var c = g[i];
            if (remaining.TryGetValue(c, out var count) && count > 0)
            {
                pattern[i] = 'Y';
                remaining[c] = count - 1;
            }
            else
            {
                pattern[i] = 'X';
            }
        }

        return new Feedback { Guess = g, Pattern = new string(pattern) };
    }
}

