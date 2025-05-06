using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.PublicException;

namespace Domain.OsagoAggregate;

public sealed class ExpiryStatus : Entity<int>
{
    public static readonly ExpiryStatus NotExpired = new(1, nameof(NotExpired).ToLowerInvariant());
    public static readonly ExpiryStatus Expired = new(2, nameof(Expired).ToLowerInvariant());

    private ExpiryStatus()
    {
    }

    private ExpiryStatus(int id, string name) : this()
    {
        Id = id;
        Name = name;
    }

    public string Name { get; } = null!;

    public static ExpiryStatus FromName(string name)
    {
        var status = All()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (status == null) throw new ValueIsUnsupportedException($"{nameof(name)} unknown status or null");
        return status;
    }

    public static ExpiryStatus FromId(int id)
    {
        var status = All().SingleOrDefault(s => s.Id == id);
        if (status == null) throw new ValueIsUnsupportedException($"{nameof(id)} unknown status or null");
        return status;
    }

    public static IEnumerable<ExpiryStatus> All()
    {
        return
        [
            NotExpired,
            Expired
        ];
    }

    public static bool operator ==(ExpiryStatus? a, ExpiryStatus? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Id == b.Id;
    }

    public static bool operator !=(ExpiryStatus a, ExpiryStatus b)
    {
        return !(a == b);
    }

    private bool Equals(ExpiryStatus other)
    {
        return base.Equals(other) && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is ExpiryStatus other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Id);
    }
}