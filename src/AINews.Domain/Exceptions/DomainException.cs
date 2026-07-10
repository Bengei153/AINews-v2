namespace AINews.Domain.Exceptions;

/// <summary>Thrown when an entity's invariants would be violated.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
