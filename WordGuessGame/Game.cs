using WordGuessGame.Exceptions;
using WordGuessGame.Models;
using WordGuessGame.Services;

namespace WordGuessGame;

public sealed class Game
{
    private readonly Func<Difficulty, string> _getRandomWord;
    private readonly GuessValidator _guessValidator;
    private readonly FeedbackGenerator _feedbackGenerator;
    private readonly Difficulty _difficulty;
    private readonly int _maxAttempts;

    public Game(
        Func<Difficulty, string> getRandomWord,
        GuessValidator guessValidator,
        FeedbackGenerator feedbackGenerator,
        Difficulty difficulty,
        int maxAttempts)
    {
        _getRandomWord = getRandomWord;
        _guessValidator = guessValidator;
        _feedbackGenerator = feedbackGenerator;
        _difficulty = difficulty;
        _maxAttempts = maxAttempts;
    }

    public GameResult Run()
    {
        var hiddenWord = _getRandomWord(_difficulty);
        var previousGuesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"Difficulty: {_difficulty}");
        Console.WriteLine($"Attempts: {_maxAttempts}\n");

        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            Console.Write($"Attempt {attempt}/{_maxAttempts} - Enter guess: ");

            string guess;
            try
            {
                var raw = Console.ReadLine();
                if (raw is null)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Input stream closed. Ending game.");
                    Console.ResetColor();

                    return new GameResult
                    {
                        HiddenWord = hiddenWord,
                        Difficulty = _difficulty,
                        IsWin = false,
                        AttemptsUsed = attempt - 1,
                        MaxAttempts = _maxAttempts,
                        Score = 0,
                    };
                }

                guess = _guessValidator.ValidateOrThrow(raw);

                if (!previousGuesses.Add(guess))
                    throw new InvalidGuessException("Duplicate guess. Try a different word.");
            }
            catch (InvalidGuessException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                attempt--; // invalid guess does not consume an attempt
                continue;
            }

            var feedback = _feedbackGenerator.Generate(hiddenWord, guess);
            RenderFeedback(feedback);

            if (string.Equals(guess, hiddenWord, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(GetAttemptComment(attempt));
                Console.ResetColor();

                var score = CalculateScore(isWin: true, attemptUsed: attempt, difficulty: _difficulty);
                Console.WriteLine($"Score: {score}");
                return new GameResult
                {
                    HiddenWord = hiddenWord,
                    Difficulty = _difficulty,
                    IsWin = true,
                    AttemptsUsed = attempt,
                    MaxAttempts = _maxAttempts,
                    Score = score,
                };
            }

            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Out of attempts!");
        Console.ResetColor();
        Console.WriteLine($"The hidden word was: {hiddenWord}");

        var lossScore = CalculateScore(isWin: false, attemptUsed: _maxAttempts, difficulty: _difficulty);
        Console.WriteLine($"Score: {lossScore}");

        return new GameResult
        {
            HiddenWord = hiddenWord,
            Difficulty = _difficulty,
            IsWin = false,
            AttemptsUsed = _maxAttempts,
            MaxAttempts = _maxAttempts,
            Score = lossScore,
        };
    }

    private static void RenderFeedback(Feedback feedback)
    {
        // Line 1: letters (colored)
        for (var i = 0; i < 5; i++)
        {
            Console.ForegroundColor = feedback.Pattern[i] switch
            {
                'G' => ConsoleColor.Green,
                'Y' => ConsoleColor.Yellow,
                _ => ConsoleColor.DarkGray,
            };
            Console.Write($"{feedback.Guess[i]} ");
        }
        Console.ResetColor();
        Console.WriteLine();

        // Line 2: G/Y/X (colored)
        for (var i = 0; i < 5; i++)
        {
            Console.ForegroundColor = feedback.Pattern[i] switch
            {
                'G' => ConsoleColor.Green,
                'Y' => ConsoleColor.Yellow,
                _ => ConsoleColor.DarkGray,
            };
            Console.Write($"{feedback.Pattern[i]} ");
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    private static string GetAttemptComment(int attempt) =>
        attempt switch
        {
            1 => "Genius!",
            2 => "Excellent!",
            3 => "Great job!",
            4 => "Good work!",
            5 => "Nice try!",
            6 => "That was close!",
            _ => "Well done!",
        };

    private static int CalculateScore(bool isWin, int attemptUsed, Difficulty difficulty)
    {
        var multiplier = difficulty switch
        {
            Difficulty.Easy => 1,
            Difficulty.Normal => 2,
            Difficulty.Hard => 3,
            _ => 1
        };

        if (!isWin)
            return 0;

        // Higher score for fewer attempts: attempt 1 => 600, attempt 6 => 100
        var baseScore = 700 - (attemptUsed * 100);
        return baseScore * multiplier;
    }
}

