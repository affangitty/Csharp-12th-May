namespace WordGuessGame;

public static class SecureConsole
{
    public static string ReadLineMasked()
    {
        var buffer = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return new string(buffer.ToArray());
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (buffer.Count > 0)
                {
                    buffer.RemoveAt(buffer.Count - 1);
                    Console.Write("\b \b");
                }

                continue;
            }

            if (key.KeyChar is not '\0' && !char.IsControl(key.KeyChar))
            {
                buffer.Add(key.KeyChar);
                Console.Write('*');
            }
        }
    }
}
