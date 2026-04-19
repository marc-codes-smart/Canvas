namespace Canvas.Common.Errors;

public sealed class DomainException(string message) : Exception(message);
