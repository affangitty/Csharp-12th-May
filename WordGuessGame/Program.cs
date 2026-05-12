using Microsoft.Extensions.Configuration;
using WordGuessGame;
using WordGuessGame.Data;
using WordGuessGame.Models;
using WordGuessGame.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var wordsFilePath = Path.Combine(AppContext.BaseDirectory, "words.txt");

var dbFactory = new DbConnectionFactory(configuration);
var initializer = new DatabaseInitializer(dbFactory);

try
{
    initializer.EnsureCreatedAndSeedWordsIfEmpty(wordsFilePath);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Database setup failed. Check PostgreSQL is running and ConnectionStrings:DefaultConnection is correct.");
    Console.WriteLine(ex.Message);
    Console.ResetColor();
    return;
}

var userRepository = new UserRepository(dbFactory);
var wordRepository = new WordRepository(dbFactory, Random.Shared);
var scoreRepository = new ScoreRepository(dbFactory);
var authService = new AuthService(userRepository);
var authFlow = new AuthConsoleFlow(authService);

var guessValidator = new GuessValidator();
var feedbackGenerator = new FeedbackGenerator();

Console.Title = "Word Guess Game";

while (true)
{
    var player = authFlow.RunUntilLoginOrExit();
    if (player is null)
        break;

    RunGameSession(player, wordRepository, scoreRepository, guessValidator, feedbackGenerator);

    Console.WriteLine();
    Console.WriteLine("You have been signed out.\n");
    Console.Write("Return to sign-in menu? (Y/N): ");
    var back = Console.ReadLine()?.Trim();
    if (back is null)
        break;
    if (!back.Equals("Y", StringComparison.OrdinalIgnoreCase))
        break;
}

static void RunGameSession(
    LoggedInPlayer player,
    WordRepository words,
    ScoreRepository scores,
    GuessValidator guessValidator,
    FeedbackGenerator feedbackGenerator)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine($"=== WORD GUESS GAME ===  ({player.Username})");
        Console.WriteLine("Guess the hidden 5-letter word in 6 attempts.\n");

        var difficulty = PromptDifficulty();

        var game = new Game(
            getRandomWord: words.GetRandomWord,
            guessValidator: guessValidator,
            feedbackGenerator: feedbackGenerator,
            difficulty: difficulty,
            maxAttempts: 6);

        var result = game.Run();

        try
        {
            scores.Insert(player.UserId, result);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nCould not save score: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.Write("Play another round? (Y/N): ");
        var replayRaw = Console.ReadLine();
        if (replayRaw is null)
            break;

        var replay = replayRaw.Trim();
        if (!replay.Equals("Y", StringComparison.OrdinalIgnoreCase))
            break;
    }
}

static Difficulty PromptDifficulty()
{
    while (true)
    {
        Console.WriteLine("Choose difficulty:");
        Console.WriteLine("1) Easy");
        Console.WriteLine("2) Normal");
        Console.WriteLine("3) Hard");
        Console.Write("Enter choice (1-3): ");

        var raw = Console.ReadLine();
        if (raw is null)
            return Difficulty.Normal;

        var input = raw.Trim();
        Console.WriteLine();

        if (input == "1") return Difficulty.Easy;
        if (input == "2") return Difficulty.Normal;
        if (input == "3") return Difficulty.Hard;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid choice. Try again.\n");
        Console.ResetColor();
    }
}
