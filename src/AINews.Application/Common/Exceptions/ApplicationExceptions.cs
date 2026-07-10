using FluentValidation.Results;

namespace AINews.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}

/// <summary>Thrown when credentials are invalid during login.</summary>
public class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message = "Invalid email or password.") : base(message) { }
}

/// <summary>Thrown when a caller attempts an action they don't have permission for.</summary>
public class ForbiddenAccessException : Exception
{
}
