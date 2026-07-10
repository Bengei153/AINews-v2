using System.ComponentModel.DataAnnotations.Schema;

namespace AINews.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides identity and a place to
/// collect domain events so handlers can react to state changes after
/// SaveChanges (see Infrastructure/Persistence/ApplicationDbContext).
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<BaseDomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(BaseDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Adds creation/modification auditing. Populated automatically by
/// ApplicationDbContext.SaveChangesAsync via ICurrentUserService/IDateTime.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}

public abstract class BaseDomainEvent
{
    public DateTimeOffset OccurredOn { get; protected set; } = DateTimeOffset.UtcNow;
}
