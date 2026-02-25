namespace PortfolioAPI.Exceptions;

public class BadRequestException(string message, Dictionary<string, string[]>? validationError = null)
    : Exception(message)
{
    public Dictionary<string, string[]>? ValidationError { get; } = validationError;
}

public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class NotAcceptableException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);
