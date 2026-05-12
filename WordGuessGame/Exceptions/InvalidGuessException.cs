namespace WordGuessGame.Exceptions;

public sealed class InvalidGuessException : Exception
{
    public InvalidGuessException(string message) : base(message) { }
}

