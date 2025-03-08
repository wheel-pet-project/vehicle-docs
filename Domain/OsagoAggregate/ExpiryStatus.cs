using CSharpFunctionalExtensions;
using Domain.SharedKernel.Exceptions.ArgumentException;

namespace Domain.OsagoAggregate;

public sealed class ExpiryStatus : Entity<int>
{
    public static readonly ExpiryStatus NotExpired = new(1, nameof(NotExpired).ToLowerInvariant());
    public static readonly ExpiryStatus Expired = new(2, nameof(Expired).ToLowerInvariant());
    
    private ExpiryStatus() { }

    private ExpiryStatus(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public string Name { get; } = null!;

    public static ExpiryStatus FromName(string name)
    {
        var status = All()
            .SingleOrDefault(s => string.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));
        if (status == null) throw new ValueOutOfRangeException($"{nameof(name)} unknown status or null");
        return status;
    }

    public static ExpiryStatus FromId(int id)
    {
        var status = All().SingleOrDefault(s => s.Id == id);
        if (status == null) throw new ValueOutOfRangeException($"{nameof(id)} unknown status or null");
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
}